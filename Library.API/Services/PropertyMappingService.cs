using System;
using System.Collections.Generic;
using System.Linq;
using Library.API.Entities;
using Library.API.Model;

namespace Library.API.Services
{
    public class PropertyMappingService : IPropertyMappingService
    {
        private Dictionary<string, PropertyMappingValue> _authorPropertyMapping =
            new Dictionary<string, PropertyMappingValue>(StringComparer.OrdinalIgnoreCase)
            {
                {"Id", new PropertyMappingValue(new List<string> {"Id"})},
                {"Genre", new PropertyMappingValue(new List<string> {"Genre"})},
                {"Age", new PropertyMappingValue(new List<string> {"DateOfBirth"}, true)},
                {"Name", new PropertyMappingValue(new List<string> {"FirstName", "LastName"})}
            };

        private IList<IPropertyMapping> propertyMappings = new List<IPropertyMapping>();

        public PropertyMappingService()
        {
            propertyMappings.Add(new PropertyMapping<AuthorDto, Author>(_authorPropertyMapping));
        }

        public Dictionary<string, PropertyMappingValue> GetPropertyMapping<TSource, TDestination>()
        {
            //get matching mapping
            var matchingMaping = propertyMappings.OfType<PropertyMapping<TSource, TDestination>>();
            var enumerable = matchingMaping as IList<PropertyMapping<TSource, TDestination>> ?? matchingMaping.ToList();
            if (enumerable.Count() == 1)
            {
                return enumerable.First()._MappingDictionary;
            }
            throw new Exception(
                $"Cannot find exact propertu mapping instance for <{typeof(TSource)}, {typeof(TDestination)}>");
        }

        public bool ValidMappingExistsFor<TSource, TDestination>(string fields)
        {
            var propertyMapping = GetPropertyMapping<TSource, TDestination>();
            if (string.IsNullOrEmpty(fields))
            {
                return true;
            }
            var fieldsAfterSplit = fields.Split(',');

            return (from field in fieldsAfterSplit
                    select field.Trim()
                    into trimmedField
                    let indexOfFirstSpace = trimmedField.IndexOf(" ")
                    select indexOfFirstSpace == -1
                        ? trimmedField
                        : trimmedField.Remove(indexOfFirstSpace))
                .All(propertyName => propertyMapping.ContainsKey(propertyName));
        }
    }
}