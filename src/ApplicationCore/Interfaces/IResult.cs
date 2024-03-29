﻿namespace ApplicationCore.Interfaces
{
    public interface IResult
    {
        IEnumerable<string> Messages { get; set; }

        bool Succeeded { get; set; }
    }

    public interface IResult<out T> : IResult
    {
        T Data { get; }
    }
}
