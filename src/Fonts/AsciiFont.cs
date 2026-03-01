using System.Globalization;
using System.Text.RegularExpressions;

namespace TRoschinsky.Common;

/// <summary>
/// Reads an ASCII character based font type, called FIGlet font, into 
/// an CLR object using .flf definition files.
/// --------------------------------------------------------------------
/// To learn more about FIGlet, please visit https://www.figlet.org.
/// Special credits to Glenn Chappell <http://www.cs.uaf.edu/~chappell/>, 
/// Frank Sheeran <http://www.bsag.ch/~fs/>, Ian Chai <http://ianchai.50megs.com/> 
/// and the spirit of the 1990s! <3
/// </summary>
public class AsciiFont
{
    private const string magicString = "flf2a";

    private readonly int maxLength;
    private readonly int smushMode;
    private readonly int commentLines;
    private readonly char hardBlank;
    private readonly char endToken = '@';

    public int CharHeight { get; private set; }
    public int CharHeightWoDesc { get; private set; }
    public string AuthorComments { get; private set; } = string.Empty;
    public Dictionary<int, string[]> AsciiLetters { get; private set; } = [];
    public AsciiFontProcessingState ProcessingState { get; private set; } = AsciiFontProcessingState.NoStarted;


    /// <summary>
    /// Tries to convert a .flf string into a font object representing this FIGlet font
    /// </summary>
    /// <param name="fontDefString">The full string-based flf font definition</param>
    /// <exception cref="ArgumentException">Throws when processing of flf input fails completely</exception>
    public AsciiFont(string fontDefString)
    {
        if (string.IsNullOrWhiteSpace(fontDefString))
        {
            ProcessingState = AsciiFontProcessingState.Failed;
            throw new ArgumentException("Unable to process font: Missing font definition.");
        }

        fontDefString = fontDefString.ReplaceLineEndings();
        string[] fontDefLines = fontDefString.Split(Environment.NewLine);
        if (fontDefLines == null || fontDefLines.Length == 0)
        {
            ProcessingState = AsciiFontProcessingState.Failed;
            throw new ArgumentException("Unable to process font: Empty font definition.");
        }

        string[]? fontHeaders = fontDefLines[0]?.Split(' ');
        if (fontHeaders == null || (!fontHeaders[0].StartsWith(magicString)))
        {
            ProcessingState = AsciiFontProcessingState.Failed;
            throw new ArgumentException("Unable to process font: Missing font header aka 'magic string'.");
        }

        try
        {
            hardBlank = fontHeaders[0].Last();
            CharHeight = Convert.ToInt32(fontHeaders[1]);
            CharHeightWoDesc = Convert.ToInt32(fontHeaders[2]);
            maxLength = Convert.ToInt32(fontHeaders[3]);
            smushMode = Convert.ToInt32(fontHeaders[4]);
            commentLines = Convert.ToInt32(fontHeaders[5]);
        }
        catch (Exception ex)
        {
            ProcessingState = AsciiFontProcessingState.Failed;
            throw new ArgumentException("Unable to process font: Font header incomplete.", ex);
        }

        int commentLineIndex;
        for (commentLineIndex = 1; commentLineIndex <= commentLines; commentLineIndex++)
        {
            AuthorComments += fontDefLines[commentLineIndex];
        }

        if (!fontDefLines[commentLineIndex].EndsWith(endToken))
        {
            endToken = fontDefLines[commentLineIndex][^1];
        }

        ExtractAsciiLetters(fontDefLines, commentLineIndex);
    }

    /// <summary>
    /// Retuns a single line of a ASCII based character by char int value
    /// </summary>
    /// <param name="charToConvert">The int equivalent to the char you want to get</param>
    /// <param name="charLine">The zero based line number of the ASCII based character</param>
    /// <returns>The line of the ASCII based character</returns>
    /// <exception cref="ArgumentOutOfRangeException">Throws when wanted line is not in range</exception>
    public string GetAsciiLetter(char charToConvert, int charLine)
    {
        Int16 charVal = Convert.ToInt16(charToConvert);
        if (charLine < 0 || charLine >= CharHeight)
        {
            throw new ArgumentOutOfRangeException($"Char #{charToConvert}, line {charLine}.");
        }
        else if (AsciiLetters.TryGetValue(charVal, out string[]? charLines))
        {
            return charLines[charLine];
        }
        else
        {
            return AsciiLetters[0][charLine];
        }
    }

    /// <summary>
    /// Test for special characters defined explicitly
    /// </summary>
    /// <param name="charDescription">Reference to the char identifier</param>
    /// <param name="specialChar">If found, the special char identifier, otherwise -1</param>
    /// <returns>True if special character was found</returns>
    private static bool CheckForSpecialChar(string charDescription, char endToken, out int specialChar)
    {
        specialChar = -1;
        Match match = Regex.Match(charDescription, $"^(0x)*[0-9a-fA-F]+  .*(?<!{endToken})$");
        if (match.Success)
        {
            string matchNum = match.Value[..match.Value.IndexOf("  ")];
            if (matchNum.Contains("0x"))
            {
                specialChar = int.Parse(matchNum.Trim(['0', 'x']), NumberStyles.HexNumber);
            }
            else
            {
                specialChar = int.Parse(matchNum);
            }
        }
        return match.Success;
    }

    /// <summary>
    /// Extracts all the ASCII letters from .flf defintion
    /// </summary>
    /// <param name="fontDefLines">The full .flf definition</param>
    /// <param name="startAt">Entry point for parsing</param>
    private void ExtractAsciiLetters(string[] fontDefLines, int startAt)
    {
        string endAsciiLetter = new([endToken, endToken]);
        int currentChar = 32;

        AsciiLetters[0] = new string[CharHeight];
        for (int i = 0; i < CharHeight; i++)
        {
            AsciiLetters[0][i] = i == 0 ? " <nal>  " : (i < CharHeightWoDesc ? AsciiLetters[0][0] : "       ");
        }
        ProcessingState = AsciiFontProcessingState.Partial;

        try
        {
            for (int fontDefLineIndex = startAt; fontDefLineIndex < fontDefLines.Length; fontDefLineIndex++)
            {
                
if(currentChar == 128)
{
    System.Threading.Thread.Sleep(3);
}

                if (CheckForSpecialChar(fontDefLines[fontDefLineIndex], endToken, out var specialChar))
                {
                    currentChar = specialChar;
                    fontDefLineIndex++;
                }

                AsciiLetters[currentChar] = new string[CharHeight];
                int asciiLetterLineIndex = 0;
                string asciiLetterLine;
                while (asciiLetterLineIndex < CharHeight)
                {
                    try
                    {
                        asciiLetterLine = fontDefLines[fontDefLineIndex + asciiLetterLineIndex] ?? string.Empty;
                        AsciiLetters[currentChar][asciiLetterLineIndex] = asciiLetterLine.TrimEnd(endToken).Replace(hardBlank, ' ');

                        if (asciiLetterLine.EndsWith(endAsciiLetter))
                        {
                            break;
                        }
                        asciiLetterLineIndex++;
                    }
                    catch (Exception)
                    {
                        break;
                    }
                }
                fontDefLineIndex += asciiLetterLineIndex;
                currentChar++;
            }
            ProcessingState = AsciiFontProcessingState.Successful;
        }
        catch (Exception ex) { ProcessingState = AsciiFontProcessingState.Failed; System.Diagnostics.Debug.WriteLine(ex.Message); }
    }
}

/// <summary>
/// Processing status information
/// </summary>
public enum AsciiFontProcessingState
{
    Successful,
    Partial,
    Failed,
    NoStarted
}