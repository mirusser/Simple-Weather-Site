using System;

namespace Models.Internal
{
    public class CustomException : Exception
    {
        public string? Code { get; set; }
        public Type? Type { get; set; }

        public CustomException() : base()
        {
        }

        public CustomException(string? message) : base(message)
        {
        }

        public CustomException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        public CustomException(
            string code,
            string message,
            params object[] args) : base(string.Format(message, args), null)
        {
            Code = code;
        }
    }
}