using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unclassified.TxLib;
using static Ovresko.Generix.Core.Translations.OvTranslate;

namespace Ovresko.Generix.Core.Framework.Helpers
{
   public static class NumberToWords
    {
        public static String ConvertToWords(this int number)
        {
            return NumberToWords.ConvertToWords(Convert.ToDouble(number));
        }

        public static String ConvertToWords(this decimal number)
        {
           return NumberToWords.ConvertToWords(decimal.ToDouble(number));
        }
        public static String ConvertToWords(this double number)
        {
            string numb = number.ToString();
            String val = "", wholeNo = numb, points = "", andStr = "", pointStr = "";
            String endStr = _("Only");
            try
            {
                int decimalPlace = numb.IndexOf(".");
                if (decimalPlace > 0)
                {
                    wholeNo = numb.Substring(0, decimalPlace);
                    points = numb.Substring(decimalPlace + 1);
                    if (Convert.ToInt32(points) > 0)
                    {
                        andStr = _("and");// just to separate whole numbers from points/cents  
                        endStr = _("Paisa") + endStr;//Cents  
                        pointStr = ConvertWholeNumber(points);
                    }
                }
                val = String.Format("{0} {1} {2} {3}", ConvertWholeNumber(wholeNo).Trim(), andStr, pointStr, endStr);
            }
            catch { }
            return val;
        }
        private static String ConvertWholeNumber(String Number)
        {
            string word = "";
            try
            {
                bool beginsZero = false;//tests for 0XX
                bool isDone = false;//test if already translated
                double dblAmt = (Convert.ToDouble(Number));
                //if ((dblAmt > 0) && number.StartsWith("0"))
                if (dblAmt > 0)
                {//test for zero or digit zero in a nuemric
                    beginsZero = Number.StartsWith("0");

                    int numDigits = Number.Length;
                    int pos = 0;//store digit grouping
                    String place = "";//digit grouping name:hundres,thousand,etc...
                    switch (numDigits)
                    {
                        case 1://ones' range

                            word = ones(Number);
                            isDone = true;
                            break;
                        case 2://tens' range
                            word = tens(Number);
                            isDone = true;
                            break;
                        case 3://hundreds' range
                            pos = (numDigits % 3) + 1;
                            place = _("Hundred");
                            break;
                        case 4://thousands' range
                        case 5:
                        case 6:
                            pos = (numDigits % 4) + 1;
                            place = _("Thousand");
                            break;
                        case 7://millions' range
                        case 8:
                        case 9:
                            pos = (numDigits % 7) + 1;
                            place = _("Million");
                            break;
                        case 10://Billions's range
                        case 11:
                        case 12:

                            pos = (numDigits % 10) + 1;
                            place = _("Billion");
                            break;
                        //add extra case options for anything above Billion...
                        default:
                            isDone = true;
                            break;
                    }
                    if (!isDone)
                    {//if transalation is not done, continue...(Recursion comes in now!!)
                        if (Number.Substring(0, pos) != "0" && Number.Substring(pos) != "0")
                        {
                            try
                            {
                                word = ConvertWholeNumber(Number.Substring(0, pos)) + place + ConvertWholeNumber(Number.Substring(pos));
                            }
                            catch { }
                        }
                        else
                        {
                            word = ConvertWholeNumber(Number.Substring(0, pos)) + ConvertWholeNumber(Number.Substring(pos));
                        }

                        //check for trailing zeros
                        //if (beginsZero) word = " and " + word.Trim();
                    }
                    //ignore digit grouping names
                    if (word.Trim().Equals(place.Trim())) word = "";
                }
            }
            catch { }
            return word.Trim();
        }

        private static String tens(String Number)
        {
            int _Number = Convert.ToInt32(Number);
            String name = null;
            switch (_Number)
            {
                case 10:
                    name = _("Ten");
                    break;
                case 11:
                    name = _("Eleven");
                    break;
                case 12:
                    name = _("Twelve");
                    break;
                case 13:
                    name = _("Thirteen");
                    break;
                case 14:
                    name = _("Fourteen");
                    break;
                case 15:
                    name =_( "Fifteen");
                    break;
                case 16:
                    name =_( "Sixteen");
                    break;
                case 17:
                    name = _("Seventeen");
                    break;
                case 18:
                    name = _("Eighteen");
                    break;
                case 19:
                    name = _("Nineteen");
                    break;
                case 20:
                    name =_( "Twenty");
                    break;
                case 30:
                    name = _("Thirty");
                    break;
                case 40:
                    name = _("Fourty");
                    break;
                case 50:
                    name =_( "Fifty");
                    break;
                case 60:
                    name = _("Sixty");
                    break;
                case 70:
                    name = _("Seventy"); ;
                    break;
                case 80:
                    name =_( "Eighty");
                    break;
                case 90:
                    name = _("Ninety");
                    break;
              
                default:
                    if (_Number > 0)
                    {
                        if(_Number >= 70 && _Number<= 79 && Tx.CurrentThreadCulture.Contains("fr"))
                        {
                            name = tens("60") + " " + tens("1"+Number.Substring(1));
                        }
                        else
                        {

                            name = tens(Number.Substring(0, 1) + "0") + " " + ones(Number.Substring(1));

                        }
                    }
                    break;
            }
            return name;
        }
        private static String ones(String Number)
        {
            int _Number = Convert.ToInt32(Number);
            String name = "";
            switch (_Number)
            {

                case 1:
                    name = _("One");
                    break;
                case 2:
                    name = _("Two");
                    break;
                case 3:
                    name = _("Three");
                    break;
                case 4:
                    name = _("Four");
                    break;
                case 5:
                    name = _("Five");
                    break;
                case 6:
                    name = _("Six");
                    break;
                case 7:
                    name = _("Seven");
                    break;
                case 8:
                    name = _("Eight");
                    break;
                case 9:
                    name = _("Nine");
                    break;
            }
            return name;
        }

        private static String ConvertDecimals(String number)
        {
            String cd = "", digit = "", engOne = "";

            if(number.Length == 1)
            {
                cd = " " + ones(number);
            }else if(number.Length == 2)
            {
                cd = " " + tens(number);
            }
            else if (number.Length == 3)
            {
                cd = " " + tens(number);
            }
            //for (int i = 0; i < number.Length; i++)
            //{
            //    digit = number[i].ToString();
            //    if (digit.Equals("0"))
            //    {
            //        engOne = _("Zero");
            //    }
            //    else
            //    {
            //        engOne = ones(digit);
            //    }
            //    cd += " " + engOne;
            //}
            return cd;
        }
    }
}
