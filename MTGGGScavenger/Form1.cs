using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net.Http;
using System.Net;
using System.Windows.Markup;
using HtmlAgilityPack;
using System.Text.RegularExpressions;

namespace MTGGGScavenger
{
    public partial class Form1 : Form
    {

        int[] sets = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 34, 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 48, 49, 50, 51, 52, 53, 54, 55, 57, 58, 59, 60, 61, 62, 63, 64, 65, 66, 67, 68, 69, 70, 71, 72, 73, 74, 75, 83, 85, 86, 87, 88, 89, 90, 91, 92, 93, 94, 96, 97, 98, 99, 100, 101, 102, 103, 104, 105, 111, 112, 113, 114, 117, 118, 204, 205, 218, 261, 295, 298, 299, 300, 301, 302, 303, 304, 309, 310, 311, 312, 314, 315, 316, 317, 318, 319, 320, 321, 322, 324, 326, 327, 328, 329, 343, 344, 347 };
        public Form1()
        {
            
            InitializeComponent();
            this.Text = "MTG.GG Buylist Scraper";
        }

        private void button1_Click(object sender, EventArgs e)
        {
            parseMTGGG();
        }

        private async void parseMTGGG()
        {
            richTextBox1.Text = "";
            //326 sets start at 4 for revised
            for(int i = 3; i < sets.Length; i++)
            {
                string url = "http://www.quietspeculation.com/tt3/?action=editionPrice&set_id=" + sets[i] + "&max_spread_pct=" + Convert.ToInt32(textBox1.Text) + "&max_spread_usd=&min_spread_pct=&min_spread_usd=&min_buy=&min_sell=&max_buy=&max_sell6";
                            HttpClient http = new HttpClient();
                            var response = await http.GetByteArrayAsync(url);
                String source = Encoding.GetEncoding("utf-8").GetString(response, 0, response.Length - 1);
                source = WebUtility.HtmlDecode(source);
                HtmlAgilityPack.HtmlDocument resultat = new HtmlAgilityPack.HtmlDocument();
                resultat.LoadHtml(source);
                List<HtmlNode> toftitle = resultat.DocumentNode.Descendants().Where(x => (x.Name == "div" && x.Attributes["class"] != null && x.Attributes["class"].Value.Contains("tab-pane active"))).ToList();
                
                if(toftitle.Count >= 1)
                {
                    var card = toftitle[0].InnerText;
                    char firstLetter;
                    foreach (Match match in Regex.Matches(card, "\n\t\t\t\n\t\t\t[A-Z]"))
                    {

                        Console.WriteLine(match.Value[match.Length - 1]);
                        firstLetter = match.Value[match.Length - 1];
                        card = card.Replace(match.Value.ToString(), ":" + firstLetter.ToString());
                    }
                    card = card.Replace('\t', ' ');
                    card = card.Replace('\n', ' ');
                    // card = Regex.Replace(card, "\n\t\t\t[A-Z]", "+");
                    String stripped = Regex.Replace(card, "Card NameRarityBuySell", "");
                    stripped = Regex.Replace(stripped, @"\t|\r", "");
                    stripped = stripped.Trim();
                    stripped = Regex.Replace(stripped, ":", "\n");
                    if (stripped != "")
                        richTextBox1.Text += stripped + "\n";
                }



                label2.Invoke(new Action(delegate() { label2.Text = ("On Set: " + i + " of " + (sets.Length - 1)); }));

            }



            string newString = "";
                            string newline = "";
                string profitLine = "";
                double profit = 0;
            foreach(string line in richTextBox1.Text.Split('\n'))
            {
                
                char[] nums = "0123456789".ToCharArray();
                string buyValue = "";
                string value = "";
                string name = "";
                

                if(line.Length > 4)
                {
                     value = line.Substring(line.LastIndexOf(' ') + 1);
                     name = line.Substring(0, line.IndexOf("  "));
                    int index = line.IndexOfAny(nums);
                    int stop = line.IndexOf(" ",index);
                    buyValue = line.Substring(index,stop - index);
                    double newProfit = Convert.ToDouble(buyValue) - Convert.ToDouble(value);
                    newline = String.Format("{0,-10} {1," + (30 - name.Length).ToString() + "} {2,5}", name, buyValue, value);
                    if (newProfit > profit)
                    {
                        profit = newProfit;
                        profitLine = newline;
                    }
                }


               newString += (String.Format("{0,-10} {1," +( 30 - name.Length).ToString()+"} {2,5}",name ,buyValue, value));
               newString += "\n";
            }
            richTextBox1.Text = newString;
            richTextBox1.Text += "\nDone!\nHighest Profit is: \n" +profitLine ;

        }
    }
}
