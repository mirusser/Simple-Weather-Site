using System;
using Models.Base;

namespace Models.Internal
{
    public class SentMailRequest : BaseMailRequest
    {
        public DateTime SendingDate { get; set; }
    }
}