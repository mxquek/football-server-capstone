﻿
namespace FootballServerCapstone.Core
{
    public class Response
    {
        public bool Success { get; set; }
        public List<string> Message { get; set; }
    }
    public class Response<T> : Response
    {
        public T Data { get; set; }
    }
}
