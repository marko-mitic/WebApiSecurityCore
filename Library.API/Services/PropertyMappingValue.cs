﻿using System.Collections.Generic;

namespace Library.API.Services
{
    public class PropertyMappingValue
    {
        public PropertyMappingValue(IEnumerable<string> destinationProperties, bool revert = false)
        {
            DestinationProperties = destinationProperties;
            Revert = revert;
        }

        public IEnumerable<string> DestinationProperties { get; private set; }
        public bool Revert { get; private set; }
    }
}