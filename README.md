## WebApiSecurityCoreWebApi in .net Core Security

For authorization in .net Core WebApi2, typically some sort of identity server is used, which can be one of : 
*	Identity server 4
*	Azure active directory
*	Auth0
*	OpenIddict
*	Google OAuth 2.o0
*	Facebook Login
* Etc.

When an anonymous user sends a request to a web api controller/method that is annotated with [Authorize], the method should either return 401 Unauthorised response, together with a redirect link to a login/register page if applicable. This can be and external(Google, Facebook,Twiter..) or an internal authorization provider.  When the user enters a u/p combination and successfully logs in, an authorization token is generated, typically with an expiry window, saved on the server and returned to the user. On subsequent requests, user needs to add the “Authorization” header, with <type> <token> for it’s value, ex : 

```http
Authorization: Bearer mZ1edKKACtPAb7zGlwSzvs72PvhAbGmB8K1ZrGxpcNM
```

On the server, inside the startup class, some services and middleware need to be configured in order to use authorization, and some additional helper calluses as well.
Example for JwtBearerAuthentication:
Create a static class to hold options used when making the tokens,  and another static class used for generating RSA keys.
Example for TokenAuthOptions class

```C#
    public static class TokenAuthOptions
    {
        public static string Audience { get; } = "ExampleAudience";
        public static string Issuer { get; } = "ExampleIssuer";
        public static RsaSecurityKey Key { get; } = new RsaSecurityKey(RSAKeyHelper.GenerateKey());
        public static SigningCredentials SigningCredentials { get; } =
            new SigningCredentials(Key, SecurityAlgorithms.RsaSha256Signature);
        public static TimeSpan ExpiresSpan { get; } = TimeSpan.FromMinutes(30);
    }
```

Example of an key helper class used for generating keys.

```C#
    public static class RSAKeyHelper
    {
          public static RSAParameters GenerateKey()
          {
              using (var key = new RSACryptoServiceProvider(2048))
              {
                  return key.ExportParameters(true);
              }
          }
    }
```

Next, inside the ConfigureServices method in Startup class, we call AddAuthorization method on services  parameter, passing in the AuthorizationOptions object with some properties set that tell the server which type of Authroization is used. 
Example code:

```#
services.AddAuthorization(auth =>
{
  auth.AddPolicy("Bearer", new AuthorizationPolicyBuilder()
      .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme)
      .RequireAuthenticatedUser().Build());
});
```

Next, in Configure method of the Startup class, we add the JwtBearer authentication middleware, with JwtBearerOptions object injected, and we need to make sure we add it as the first or one of the first middlewares, since we want to first authenticate the user, then allow other pieces of middleware to procces the request.
The example code:

```C#
  var options = new JwtBearerOptions
  {
      TokenValidationParameters =
      {
          IssuerSigningKey = TokenAuthOptions.Key,
          ValidAudience = TokenAuthOptions.Audience,
          ValidIssuer = TokenAuthOptions.Issuer,
          ValidateIssuerSigningKey = true,
          ValidateLifetime = true,
          ClockSkew = TimeSpan.FromMinutes(0)
      }
  };
  app.UseJwtBearerAuthentication(options);
```

Afterwards we add some sort of login controller to redirect anonymous users to, where they can get their auth token when they successfully log in. This can be some external provider, or we can use .net IdentityUser and UserManager.

Also, we can use .net IdentityRole and RoleManager to authorize user access to specific parts of the application. This is done by adding Roles parameter to the Authorize annotation. Ex [Authorize(Roles = “administrator, manager, …)]. Roles have to exist in the database.

When a user successfilly logs in, if we are using our own token creation system, we need to generate a token using a ClaimsIdentity object, passed into a CreateToken method on the JwtSecurityTokenHandler object.
Example of a key generator:

```C#
private static string GenerateToken(User user, DateTime expiresIn)
{
    var handler = new JwtSecurityTokenHandler();
    var identity = new ClaimsIdentity(
        new GenericIdentity(user.UserName, "TokenAuth"),
        new[]
        {
            new Claim("ID", user.Id)
        }
    );
    var securityToken = handler.CreateToken(new SecurityTokenDescriptor
    {
        Issuer = TokenAuthOptions.Issuer,
        Audience = TokenAuthOptions.Audience,
        SigningCredentials = TokenAuthOptions.SigningCredentials,
        Subject = identity,
        Expires = expiresIn
    });
    return handler.WriteToken(securityToken);
}
```

It should be noted that there exists an helper library for Angular called Angular-jwt that makes it easyer to store
tokens on client side. Link down below

## Microservices security with NServiceBus

Most of the security conserns of microservices when using NServiceBus are dealt with by 
the transport and persistence layers that underlie NServiceBus. As for securing messages themselves, 
there are two ways to achieve this. You can encrypt some specific properties of the message, or you can encrypt the whole message body.

### Encrypting message properties
There are two ways to encrypt a specific property of a message. The first, conventional one, is to create and encryption service, ex RijendaelEncryptionService with a key indetifier and a key, and then call EnableMessagePropertyEncription on the endpointConfiguration instance and pass in the encryption service and the property information of the property/properties we  want to encrypt.
Example:

```C#
public class MyMessage : IMessage
{
   public string MyEncryptedProperty { get; set; }
}
```

Encrypt MyEncryptedProperty.

```C#
var ascii = Encoding.ASCII;//Encoding can be ASCII or Base64. Base64 is recommended 
var encryptionService = new RijndaelEncryptionService(
    encryptionKeyIdentifier: "2015-10",
    key: ascii.GetBytes("gdDbqRpqdRbTs3mhdZh9qCaDaxJXl+e6"));

endpointConfiguration.EnableMessagePropertyEncryption(
    encryptionService: encryptionService,
    encryptedPropertyConvention: propertyInfo =>
    {
        return propertyInfo.Name.EndsWith("EncryptedProperty");
    }
);
```


The other way is to use EncryptedString class form NServiceBus.Encryption.MessageProperty namespace, 
this will automatically encrypt the property.
 
 
```C#
using NServiceBus;
using NServiceBus.Encryption.MessageProperty;

public class MyMessage : IMessage
{
    public EncryptedString MyEncryptedProperty { get; set; }
}
```

### Note about key indetifiers.
We pass in a list of keys in the RijndaelEncryptionService, and assign each of them a key identifier. 
Identifiers should be unique, good examples are timestamps or GUIDs. Each message needs to carry it’s encryption key identifier in it’s header. If it doesn’t. NServiceBus will attempt to decrypt the message by using all of the keys it has in it’s configuration, and move the message to the error queue if none of the keys succeed to decrypt the message. Also, when using multiple keys, it’s possible to use new keys without changing the handlers, since NServiceBus will try to decrypt the message with the new keys that are added, but they do have to be added to the endpoint configuration in the receiving endpoint.


#### Helpfull links:
* [Auth0 wiht .net Core example](https://auth0.com/blog/asp-dot-net-core-authentication-tutorial/)

* [Angular-jwt on github](https://github.com/auth0/angular2-jwt/)

* [NServiceBus security rundown](https://docs.particular.net/nservicebus/security/)

* [Enabling social auth (google,facebook,twiter etc)](https://docs.microsoft.com/en-us/aspnet/core/security/authentication/social/)

* [IndentityServer4](https://identityserver4.readthedocs.io/en/release/)

* [Web Api 2.0 Security Course On PluralSight](https://app.pluralsight.com/player?course=webapi-v2-security&author=dominick-baier&name=webapi-v2-security-m2-httpsecurity)
