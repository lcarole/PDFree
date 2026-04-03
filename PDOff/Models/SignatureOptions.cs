namespace PDOff.Models;

public record SignatureOptions(
    SignPageTarget PageTarget = SignPageTarget.LastPage,
    int SpecificPage = 1,
    float X = 0f,
    float Y = 0f,
    float Width = 200f,
    float Height = 80f);
