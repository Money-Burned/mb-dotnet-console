using System.Reflection;
using System.Text;

namespace TRoschinsky.Common;

/// <summary>
/// Converts single lines of plain text into an character based representation 
/// made of a given multiple-ASCII-characters font type, called FIGlet font.
/// --------------------------------------------------------------------
/// To learn more about FIGlet, please visit https://www.figlet.org.
/// Special credits to Glenn Chappell <http://www.cs.uaf.edu/~chappell/>, 
/// Frank Sheeran <http://www.bsag.ch/~fs/>, Ian Chai <http://ianchai.50megs.com/> 
/// and the spirit of the 1990s! <3
/// </summary>
public class AsciiText
{
    private string[]? TextOutput { get; set; }
    private readonly int letterSpace;
    private int Height { get { return FontType?.CharHeight ?? 0; } }
    private int Width { get { return TextOutput == null ? 0 : TextOutput!.Max(l => l?.Length ?? 0); } }

    private string textInput = string.Empty;
    public string TextInput { get { return textInput; } set { SetAsciiText(value); } }
    public AsciiFont FontType { get; }
    public List<string> FontTypeNamesAvailable = [];

    /// <summary>
    /// Constructor for font reference by index to embedded resource
    /// </summary>
    /// <param name="fontIndex">Zero based index for an embedded font resource. For available fonts check 'FontTypeNamesAvailable'.</param>
    /// <param name="letterSpacing">Number of spaces between letters</param>
    /// <exception cref="FileLoadException">Throws when font could not be loaded</exception>
    public AsciiText(int fontIndex = 0, int letterSpacing = 1)
    {
        try
        {
            letterSpace = letterSpacing < 0 ? 0 : letterSpacing;
            FontTypeNamesAvailable = [.. Assembly.GetExecutingAssembly().GetManifestResourceNames()];
            FontType = LoadFont(FontTypeNamesAvailable[fontIndex]);
        }
        catch (Exception ex)
        {
            throw new FileLoadException($"Failed to load embedded font resource at index {fontIndex}: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Constructor for font reference by name to embedded resource
    /// </summary>
    /// <param name="fontIndex">Qualified name of font resource. For available fonts check 'FontTypeNamesAvailable'.</param>
    /// <param name="letterSpacing">Number of spaces between letters</param>
    /// <exception cref="FileLoadException">Throws when font could not be loaded</exception>
    public AsciiText(string fontName, int letterSpacing = 1)
    {
        try
        {
            letterSpace = letterSpacing < 0 ? 0 : letterSpacing;
            FontTypeNamesAvailable = [.. Assembly.GetExecutingAssembly().GetManifestResourceNames()];
            FontType = LoadFont(fontName);
        }
        catch (Exception ex)
        {
            throw new FileLoadException($"Failed to load embedded font resource with name {fontName}: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Loading font from embedded resource
    /// </summary>
    /// <param name="fontName">Name of the resource</param>
    /// <returns>If sucessful the ASCII font, ready to use</returns>
    private static AsciiFont LoadFont(string fontName)
    {
        AsciiFont loadedFont;
        using (Stream? stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(fontName))
        using (StreamReader sr = new(stream!))
        {
            string fontString = sr.ReadToEnd();
            loadedFont = new AsciiFont(fontString);
        }
        return loadedFont;
    }

    /// <summary>
    /// Setter method for conversion from standard characters to ASCII characters
    /// </summary>
    /// <param name="textToConvertToAscii">Text to convert to ASCII text</param>
    /// <exception cref="ArgumentException">Throws when using multi line strings</exception>
    private void SetAsciiText(string? textToConvertToAscii)
    {
        if (textToConvertToAscii == null)
        {
            textInput = string.Empty;
            return;
        }

        textToConvertToAscii.ReplaceLineEndings();
        if (textToConvertToAscii.Contains(Environment.NewLine))
        {
            throw new ArgumentException("Please use single line input!");
        }

        textInput = textToConvertToAscii;
        TextOutput = new string[FontType.CharHeight];

        if (textInput.Length == 0)
        {
            return;
        }

        for (int currentLine = 0; currentLine < Height; currentLine++)
        {
            StringBuilder sb = new();
            foreach (char currentChar in textInput)
            {
                sb.Append(FontType.GetAsciiLetter(currentChar, currentLine));
                sb.Append(new string(' ', letterSpace));
            }
            TextOutput[currentLine] = sb.ToString();
        }
    }

    /// <summary>
    /// Returns the converted ASCII text output
    /// </summary>
    /// <returns>ASCII text based on TextInput</returns>
    public override string ToString()
    {
        if (TextOutput != null)
        {
            StringBuilder sb = new(Height * (Width + 1));
            foreach (string line in TextOutput)
            {
                sb.AppendLine(line);
            }

            return sb.ToString().TrimEnd();
        }
        return string.Empty;
    }
}
