using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FunPlusEssentials
{
    public static class PlagueLocales
    {
        public static string restartingText, roundStartsText, virusText, abilityRageText, abilityInvText, abilityMedkitText, usePresstext, survivorsWinText, drawText, infectedWinText, classText,
            openSupplyText;
        public static string[] pickupSupplyText;
        public static void SetLanguage(string language)
        {
            if (language.ToLower() == "ru")
            {
                Russian();
            }
            else
            {
                English();
            }
        }
        public static void Russian()
        {
            restartingText = "Перезапуск...";
            roundStartsText = "Инфекция начнется через";
            virusText = "Вирус витает в воздухе...";
            abilityRageText = "Ярость [G]";
            abilityInvText = "Невидимость [G]";
            abilityMedkitText = "Аптечка [G]";
            usePresstext = "Использовать [E]";
            survivorsWinText = "Выжившие победили заразу...";
            drawText = "На этот раз ничья...";
            infectedWinText = "Вирус погубил весь мир...";
            classText = "Класс:";
            openSupplyText = "открыл ящик";
            pickupSupplyText = new string[] { "нашел", "в ящике" };
        }
        public static void English()
        {
            restartingText = "Restarting...";
            roundStartsText = "Infection will start in";
            virusText = "The virus has been set loose...";
            abilityRageText = "Rage [G]";
            abilityInvText = "Invisibility [G]";
            abilityMedkitText = "Medkit [G]";
            usePresstext = "Use [E]";
            survivorsWinText = "Survivors defeated the plague...";
            drawText = "No one won...";
            infectedWinText = "Infected have taken over the world...";
            classText = "Class:";
            openSupplyText = "opened supply box";
            pickupSupplyText = new string[] { "found", "in supply box" };
        }
    }
}
