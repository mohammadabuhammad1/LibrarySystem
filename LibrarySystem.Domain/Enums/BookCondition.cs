using System.Text.Json.Serialization;

namespace LibrarySystem.Domain.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum BookCondition
{
    none = 0,
    Excellent = 1,    
    Good = 2,         
    Fair = 3,         
    Poor = 4,         
    Damaged = 5,      
    Lost = 6          
}