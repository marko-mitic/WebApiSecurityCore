using System.Collections.Generic;

namespace Library.API.Services
{
    public class PropertyMapping<TSource, TDestination> : IPropertyMapping
    {
        public PropertyMapping(Dictionary<string, PropertyMappingValue> mappingDictionary)
        {
            _MappingDictionary = mappingDictionary;
        }

        public Dictionary<string, PropertyMappingValue> _MappingDictionary { get; private set; }
    }
}