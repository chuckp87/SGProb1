using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.Services;

namespace TestWebApp3
{
    /// <summary>
    /// This Web Service has one method, GetLifeData, that returns an instance of a
    /// LifeDataInfo class to the caller with the following information:
    /// 
    /// NumPeopleInSample - the number of people in the dataset.
    /// MaxLifeYearBinValue - the maximum number of people in the dataset that were alive in the 
    ///                       corresponding year.
    /// YearsWithHighestLifeCountList - a list of years that had the MaxLifeYearBinValue
    /// YearList - a list of strings that can be used for labels with the corresponding data
    /// NumLivesList - a list of integers that specify the number of people in the dataset who
    ///                were alive during the corresponding year.
    /// 
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    [System.Web.Script.Services.ScriptService]
    public class LifeDataWS : System.Web.Services.WebService
    {
        /// <summary>
        /// GetLifeData()
        /// 
        /// Web method that returns an instance of LifeDataInfo to the caller
        /// </summary>
        [WebMethod]
        public void GetLifeData()
        {
            try
            {
                // Create a concurrent dictionary that will use a year value for a key and
                // accumulate the number of people from the dataset that were alive during that year.
                ConcurrentDictionary<int, int> lifeDataConcDictionary = new ConcurrentDictionary<int, int>();

                // Get the database connection string
                string cs = ConfigurationManager.ConnectionStrings["DBCS"].ConnectionString;
                var sampleSize = 0; // will hold the number of people in the dataset

                using (SqlConnection con = new SqlConnection(cs))
                {
                    // Get the birthyear and deathyear for each person in the sample
                    SqlCommand cmd = new SqlCommand("Select * from tblLifeData", con);
                    con.Open();
                    SqlDataReader rdr = cmd.ExecuteReader();
                    while (rdr.Read())
                    {
                        ++sampleSize;

                        // Get the birthyear and deathyear for a person in the sample
                        var birthYear = Convert.ToInt32(rdr["BirthYear"]);
                        var deathYear = Convert.ToInt32(rdr["DeathYear"]);

                        // For each year from the birthyear to the deathyear inclusive,
                        // increment by one the value stored for that year.
                        Parallel.For( birthYear,deathYear + 1,
                            dictionaryKey => 
                            {
                                lifeDataConcDictionary.AddOrUpdate(dictionaryKey,1,(key, oldValue) => oldValue + 1);
                            }
                        );
                    }
                }

                // Find the highest value corresponding to the number of people
                // alive in a given year from the dataset.
                var maxLifeYearBinValue = lifeDataConcDictionary.Values.Max();

                // Create a list of years containing the maxLifeYearBinValue.
                var yearsWithHighestLifeCount = lifeDataConcDictionary.Where(pair => pair.Value == maxLifeYearBinValue)
                                                                        .Select(pair => pair.Key);

                List<string> yearsList = new List<string>();
                List<int> valuesList = new List<int>();

                // Create a list of years and another list with the bin values for each year
                foreach (KeyValuePair<int, int> item in lifeDataConcDictionary.OrderBy(p => p.Key))
                {
                    yearsList.Add(item.Key.ToString());
                    valuesList.Add(item.Value);
                }

                // Create an instance of LifeDataInfo to pass back to the caller.
                LifeDataInfo lifeData = new LifeDataInfo()
                {
                    NumPeopleInSample = sampleSize,
                    MaxLifeYearBinValue = maxLifeYearBinValue,
                    YearsWithHighestLifeCountList = yearsWithHighestLifeCount,
                    YearList = yearsList,
                    NumLivesList = valuesList
                };

                // Serialize the data to be sent back to the caller.
                JavaScriptSerializer js = new JavaScriptSerializer();
                Context.Response.Write(js.Serialize(lifeData));
            }
            catch (Exception ex)
            {
                throw new HttpException(500, ex.Message);
            }
        }
    }
}
