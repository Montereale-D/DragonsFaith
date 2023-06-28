using System;

public class JoinCodeException : Exception
{
    public JoinCodeException()
    {
    }

    public JoinCodeException(string message) : base(message)
    {
    }
}