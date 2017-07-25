using System.Collections.Generic;

namespace Library.API.Model
{
    public class LinkedCollectionRecourceWrapperDto<T> : LinkRecourceBaseDto where T : LinkRecourceBaseDto
    {
        public LinkedCollectionRecourceWrapperDto(IEnumerable<T> value)
        {
            Value = value;
        }

        public IEnumerable<T> Value { get; set; }
    }
}