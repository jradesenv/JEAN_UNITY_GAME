using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

class Converters
{
    public static int ToValidMoveInput(float value)
    {
        int result;

        if (value == 0)
        {
            result = 0;
        }
        else if (value > 0)
        {
            result = 1;
        }
        else
        {
            result = -1;
        }

        return result;
    }

    public static string FloatToString(float value)
    {
        return value.ToString(CultureInfo.InvariantCulture);
    }

    public static float StringToFloat(string text)
    {
        return float.Parse(text, System.Globalization.CultureInfo.InvariantCulture);
    }

    public static int StringToInt(string text)
    {
        return int.Parse(text);
    }

    public static string IntToString(int value)
    {
        return value.ToString();
    }

    public static DateTime StringToDateTime(string text)
    {
        return DateTime.ParseExact(text, "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
    }

    public static string DateTimeToString(DateTime dateTime)
    {
        return dateTime.ToString("O");
    }

    public static string CharacterClassToString(Enums.CharacterClass characterClass)
    {
        return characterClass.ToString("d");
    }

    public static Enums.CharacterClass StringToCharacterClass(string characterClassValue)
    {
        return (Enums.CharacterClass)Enum.Parse(typeof(Enums.CharacterClass), characterClassValue);
    }

    public static string CharacterClassToSpriteSheetName(Enums.CharacterClass characterClass)
    {
        string spriteSheetName = "";

        switch (characterClass)
        {
            case Enums.CharacterClass.WARRIOR:
                spriteSheetName = SpriteNames.warrior;
                break;
            case Enums.CharacterClass.RANGER:
                spriteSheetName = SpriteNames.ranger;
                break;
            case Enums.CharacterClass.MAGE:
                spriteSheetName = SpriteNames.mage;
                break;
            default:
                spriteSheetName = "unknown";
                break;
        }

        return spriteSheetName;
    }
}
