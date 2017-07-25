﻿using System;

namespace Library.API.Model
{
    public class BookDto : LinkRecourceBaseDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public Guid AuthorId { get; set; }
    }
}