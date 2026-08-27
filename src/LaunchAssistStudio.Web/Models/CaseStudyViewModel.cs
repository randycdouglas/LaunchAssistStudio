namespace LaunchAssistStudio.Web.Models;

/// <summary>A portfolio case study rendered by the _CaseStudy partial.</summary>
public class CaseStudyViewModel
{
    public string Label { get; set; } = "FEATURED PROJECT";
    public string Title { get; set; } = "";
    public string Subtitle { get; set; } = "";
    public string Description { get; set; } = "";
    public string[] Tags { get; set; } = [];
    public string LogoInitials { get; set; } = "LA";
}
