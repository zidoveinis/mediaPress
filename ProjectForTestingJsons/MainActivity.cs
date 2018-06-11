using Android.App;
using Android.Widget;
using Android.OS;
using Android.Support.V7.App;
using System.Text;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using System;
using Newtonsoft.Json;
using System.Net;
using System.IO;
using Android.Content;

namespace ProjectForTestingJsons
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true)]
    public class MainActivity : AppCompatActivity
    {

        WebClient wc = new WebClient();
        string StrReaded2;
        string StrReaded;
        string content;
        string url;
        static bool kintamasis = true;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_main);
            GetJsonWeb();

            string path = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
            string filename = Path.Combine(path, "myfile.txt");

            Button saveButton = FindViewById<Button>(Resource.Id.saveButton);
         
            saveButton.Click += delegate
            {

                File.Delete(filename);
                using (var streamWriter = new StreamWriter(filename, true))
                {
                    streamWriter.WriteLine(StrReaded);
                }               
               
            };

            var streamReader = new StreamReader(filename);
            {
                content = streamReader.ReadToEnd();
                System.Diagnostics.Debug.WriteLine(content);
            }
            if (!StrReaded.Equals("") && StrReaded != null)
            {
               
                RunOnUiThread(() => Toast.MakeText(this, "StrReaded2 value: ", ToastLength.Long).Show());
                try
                {
                    JObject sourceJObject = JsonConvert.DeserializeObject<JObject>(StrReaded);
                    JObject targetJObject = JsonConvert.DeserializeObject<JObject>(content);
                    CompareObjects(sourceJObject, targetJObject);
                }
                catch (Exception ex)
                {
                    RunOnUiThread(() => Toast.MakeText(this, "Comparing Error", ToastLength.Long).Show());
                }
            }
            else
            {
                RunOnUiThread(() => Toast.MakeText(this, "StrReaded2 is NULL", ToastLength.Long).Show());
            }
            //using (var streamWriter = new StreamWriter(filename, true))
            //{
            //    streamWriter.WriteLine(StrReaded2);
            //}
        }

        private  StringBuilder CompareObjects(JObject source, JObject target)
        {
            StringBuilder returnString = new StringBuilder();
            TextView text = FindViewById<TextView>(Resource.Id.textViewTest);

            foreach (KeyValuePair<string, JToken> sourcePair in source)
            {
                
                if (sourcePair.Value.Type == JTokenType.Object && kintamasis == true)
                {
                    if (target.GetValue(sourcePair.Key) == null)
                    {
                        returnString.Append("Key " + sourcePair.Key
                                            + " not found");
                          Console.WriteLine("The show was deleted!" + sourcePair.Value);
                        text.Text = ("The show was deleted!" + sourcePair.Value);
                    }
                    else if (target.GetValue(sourcePair.Key).Type != JTokenType.Object)
                    {
                        returnString.Append("Key " + sourcePair.Key
                                            + " is not an object in target");
                        Console.WriteLine("Key" + sourcePair.Key + "is not an object in the target");
                    }
                    else if (source.GetValue(sourcePair.Key) == null)
                    {

                    }
                    else
                    {
                        returnString.Append(CompareObjects(sourcePair.Value.ToObject<JObject>(),
                            target.GetValue(sourcePair.Key).ToObject<JObject>()));
                    }
                }
                else if (sourcePair.Value.Type == JTokenType.Array)
                {
                    if (target.GetValue(sourcePair.Key) == null)
                    {
                        returnString.Append("Key " + sourcePair.Key
                                            + " not found");
                        Console.WriteLine("Key" + sourcePair.Key + "not found");

                    }
                    else
                    {
                        returnString.Append(CompareArrays(sourcePair.Value.ToObject<JArray>(),
                            target.GetValue(sourcePair.Key).ToObject<JArray>(), sourcePair.Key));
                    }
                }
                else
                {
                    JToken expected = sourcePair.Value;
                    var actual = target.SelectToken(sourcePair.Key);
                    if (actual == null)
                    {
                        //returnString.Append("Key " + sourcePair.Key
                        //                    + " not found");
                        Console.WriteLine("Key" + sourcePair.Key + "not found");
                    }
                    else
                    {
                        if (!JToken.DeepEquals(expected, actual))
                        {
                            returnString.Append("Key " + sourcePair.Key + ": "
                                                + sourcePair.Value + " !=  "
                                                + target.Property(sourcePair.Key).Value);
                            Console.WriteLine("Key {0} : {1} nelygu {2}", sourcePair.Key, sourcePair.Value, target.Property(sourcePair.Key).Value);
                       //     text.Text= ("Key {0} : {1} nelygu {2}", sourcePair.Key, sourcePair.Value, target.Property(sourcePair.Key).Value);
                            // RunOnUiThread(() => Toast.MakeText((("Key {0} : {1} nelygu {2}", sourcePair.Key, sourcePair.Value, target.Property(sourcePair.Key).Value)), ToastLength.Long).Show());
                        }
                        else
                        {
                          //  Console.WriteLine("Jsons are the same!");
                            //kintamasis = false;

                        }
                    }
                    
                }

                if (!kintamasis)
                {
                    break;
                }
            }
            return returnString;
        }

        /// <summary>
        /// Deep compare two NewtonSoft JArrays. If they don't match, returns text diffs
        /// </summary>
        /// <param name="source">The expected results</param>
        /// <param name="target">The actual results</param>
        /// <param name="arrayName">The name of the array to use in the text diff</param>
        /// <returns>Text string</returns>

        private  StringBuilder CompareArrays(JArray source, JArray target, string arrayName = "")
        {
            
            var returnString = new StringBuilder();
            for (var index = 0; index < source.Count; index++)
            {
                var expected = source[index];
                if (expected.Type == JTokenType.Object && kintamasis == true)
                {
                    var actual = (index >= target.Count) ? new JObject() : target[index];
                    returnString.Append(CompareObjects(expected.ToObject<JObject>(),
                        actual.ToObject<JObject>()));
                }
                else if (kintamasis == true)
                {
                    var actual = (index >= target.Count) ? "" : target[index];

                    if (!JToken.DeepEquals(expected, actual))
                    {
                        if (String.IsNullOrEmpty(arrayName))
                        {
                            returnString.Append("Index " + index + ": " + expected
                                                + " != " + actual);
                        }
                        else
                        {
                            returnString.Append("Key " + arrayName
                                                + "[" + index + "]: " + expected
                                                + " != " + actual);
                        }
                    }
                }

                if (!kintamasis)
                {
                    break;
                }
            }
            return returnString;
        }

        public void GetJsonWeb()
        {
            try
            {
                wc.Credentials = new NetworkCredential("robertas", "robertas-2018-05!16");
                url = "http://json.xprsdata.com/get_listings.php?channel=87&updatesFROM=1526428800";

                Stream stream = wc.OpenRead(new Uri(url));
                {
                    StreamReader reader = new StreamReader(stream);
                    {
                        StrReaded = reader.ReadToEnd();

                        StrReaded2 = StrReaded;
                        //if (StrReaded.Length != StrReaded2.Length)
                        //{
                        //    SaveData();
                        //}

                    }
                }
            }
            catch (Exception ex)
            {
                RunOnUiThread(() => Toast.MakeText(this, "Error downloading web Json", ToastLength.Long).Show());
            }
        }


        //public void SaveData()
        //{
        //    var prefs = Application.Context.GetSharedPreferences("MyApp", FileCreationMode.Private);
        //    var prefEditor = prefs.Edit();
        //    prefEditor.PutString("JsonKey", StrReaded2);
        //    prefEditor.Commit();
        //}
    }
}

