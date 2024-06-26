﻿using System.Collections.Generic;
using Microsoft.AspNetCore.Http;

namespace MQModels.Email;

public class SendEmail
{
    public string To { get; set; } = null!;
    public string Subject { get; set; } = null!;
    public string Body { get; set; } = null!;
    public string? From { get; set; }
}