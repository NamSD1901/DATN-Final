using System;
class Program {
    static void Main() {
        Console.WriteLine($"Local TimeZone: {TimeZoneInfo.Local.Id}");
        Console.WriteLine($"Now: {DateTime.Now}");
        Console.WriteLine($"UtcNow: {DateTime.UtcNow}");
        
        var dateStr = "2026-06-25T10:30:00";
        var parsed = DateTime.Parse(dateStr);
        Console.WriteLine($"Parsed: {parsed} Kind: {parsed.Kind}");
        
        var date = parsed.Date;
        Console.WriteLine($"Parsed.Date: {date} Kind: {date.Kind}");
        
        var utc = date.ToUniversalTime();
        Console.WriteLine($"ToUniversalTime: {utc} Kind: {utc.Kind}");
        
        var backToLocal = utc.ToLocalTime();
        Console.WriteLine($"Back to Local: {backToLocal} Kind: {backToLocal.Kind}");
        
        var final = backToLocal.Date.Add(parsed.TimeOfDay);
        Console.WriteLine($"Final: {final.ToString("yyyy-MM-ddTHH:mm:ss")}");
    }
}
