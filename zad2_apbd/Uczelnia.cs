using System.Text.Json.Serialization;

namespace zad2_apbd;

public class Uczelnia
{
    private DateTime createdAt;
    
    [JsonPropertyName("createdAt")]
    public string CreatedAt 
    {
        get => createdAt.ToString("dd.MM.yyyy");
        set => createdAt = DateTime.ParseExact(value, "dd.MM.yyyy", null);
    }
    
    [JsonIgnore]
    public DateTime CreatedAtDateTime
    {
        get { return createdAt; }
        set { createdAt = value; }
    }
    
    public string Author { get; set; }
    public IEnumerable<Student> Students { get; set; }
    public IEnumerable<ActiveStudies> ActiveStudies { get; set; }
}