using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Collections;
using System.Globalization;
using System.Text;
using System.Reflection;
using System.Web;
using System.Net;
using System.Xml;
using System.IO;
//using ICSharpCode.SharpZipLib.Zip;

namespace XSer
{
    public static class Test
    {
        [STAThread]
        public static void Main()
        {
            HTTPR a = new HTTPR();
            a.POI = 102;
            a.Cats = new int[] { 256, 512 };
            HTTPR c = new HTTPR();

            //Console.WriteLine(a.ToJSON());
            //Console.WriteLine();
            //string xml = a.ToXML("a");
            string xml = a.ToXML("a");
            Console.WriteLine(xml);
            HTTPR b = HTTPR.FromXML("a", xml);

            //b.FromXML("a",xml);
            //Hashtable ht = new Hashtable();
            //ht.Add("DT", DateTime.Now);
            //Console.WriteLine(XSerializer.HashTable2XML("ht",ht));
            Console.ReadLine();
        }
    }

    public class POISQLWhere
    {
        // POI_MAIN W/MIN CAT
        // SELECT ID, CATID FROM POI_MAIN RIGHT JOIN (SELECT POIID,MIN(CATID) AS CATID FROM POI_CATS GROUP BY(POIID)) CATMIN ON POI_MAIN.ID = CATMIN.POIID

        public class VIEW
        {
            public bool Hidden = false;
            public bool Premoderated = false;
            public bool Public = false;
            public bool Failed = false;
            public bool CorrectRequested = false;
            public bool MarkedToDelete = false;
            public bool Deleted = false;
        }
        public class BOUNDS
        {
            public double FLat = -90;
            public double TLat = 90;
            public double FLon = -180;
            public double TLon = 180;
        }
        public class DTRange
        {
            public DateTime From = new DateTime(1900, 1, 1);
            public DateTime To = DateTime.Now.AddYears(10);
        }
        public class SORT
        {
            public bool SortByAdd = true;
            public bool SortByMod = false;
            public bool SortByDist = false;
            public double Lat = double.NaN;
            public double Lon = double.NaN;
        }

        public int POIID = -1;
        public BOUNDS Bounds = new BOUNDS();
        public VIEW View = new VIEW();
        public int AuthorOnly = -1;
        public DTRange Created = new DTRange();
        public bool SelectNotModerated = true;
        public DTRange Moderated = new DTRange();
        public DTRange Expires = new DTRange();
        public int[] CatIN = new int[0];
        public int CatCount = -1;
        public int ModifyPOI = 0;
        public int ImportNo = -1;

        public string[] Tags = new string[0];
        public bool TagsAllIn = false;

        public string NameLike = "";

        public int UID = 0;
        public bool IsManager = false;
        public string CustomSQLWhereAND = "";
        public SORT Sort = new SORT();

        #region constructors
        public POISQLWhere(int UID)
        {
            this.UID = UID;
        }
        public POISQLWhere(int UID, bool IsManager)
        {
            this.UID = UID;
            this.IsManager = IsManager;
        }
        #endregion


        /// <summary>
        ///     Parse Select Parameters From HttpRequest
        /// </summary>
        /// <param name="Request"></param>
        public void ParseHTTPRequest(HttpRequest Request)
        {
            if ((Request["poi"] != null) && (Request["poi"] != String.Empty)) int.TryParse(Request["poi"], out POIID);
            if ((Request["flat"] != null) && (Request["flat"] != String.Empty)) Bounds.FLat = StrToFloat(Request["flat"]);
            if ((Request["tlat"] != null) && (Request["tlat"] != String.Empty)) Bounds.TLat = StrToFloat(Request["tlat"]);
            if ((Request["flon"] != null) && (Request["flon"] != String.Empty)) Bounds.FLon = StrToFloat(Request["flon"]);
            if ((Request["tlon"] != null) && (Request["tlon"] != String.Empty)) Bounds.TLon = StrToFloat(Request["tlon"]);
            if ((Request["status"] != null) && (Request["status"] != String.Empty))
            {
                string[] statuses = Request["status"].Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string st in statuses)
                {
                    int s = -1;
                    if (int.TryParse(st, out s))
                    {
                        if (s == 0) View.Hidden = true;
                        if (s == 1) View.Premoderated = true;
                        if (s == 2) View.Public = true;
                        if (s == 3) View.Failed = true;
                        if (s == 4) View.CorrectRequested = true;
                        if (s == 5) View.MarkedToDelete = true;
                        if (s == 6) View.Deleted = true;
                    };
                };
            };
            if ((Request["author"] != null) && (Request["author"] != String.Empty)) int.TryParse(Request["author"], out AuthorOnly);
            if ((Request["fadd"] != null) && (Request["fadd"] != String.Empty)) Created.From = Convert.ToDateTime(Request["fadd"]);
            if ((Request["tadd"] != null) && (Request["tadd"] != String.Empty)) Created.To = Convert.ToDateTime(Request["tadd"]);
            if ((Request["nmod"] != null) && (Request["nmod"] != String.Empty)) SelectNotModerated = Request["nmod"] == "1";
            if ((Request["fmod"] != null) && (Request["fmod"] != String.Empty)) Moderated.From = Convert.ToDateTime(Request["fmod"]);
            if ((Request["tmod"] != null) && (Request["tmod"] != String.Empty)) Moderated.To = Convert.ToDateTime(Request["tmod"]);
            if ((Request["fexp"] != null) && (Request["fexp"] != String.Empty)) Expires.From = Convert.ToDateTime(Request["fexp"]);
            if ((Request["texp"] != null) && (Request["texp"] != String.Empty)) Expires.To = Convert.ToDateTime(Request["texp"]);
            if ((Request["catin"] != null) && (Request["catin"] != String.Empty))
            {
                List<int> clist = new List<int>();
                string[] cats = Request["catin"].Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string cat in cats)
                {
                    int c = -1;
                    if (int.TryParse(cat, out c)) clist.Add(c);
                }
                CatIN = clist.ToArray();
            };
            if ((Request["mpoi"] != null) && (Request["mpoi"] != String.Empty)) int.TryParse(Request["mpoi"], out ModifyPOI);
            if ((Request["import"] != null) && (Request["import"] != String.Empty)) int.TryParse(Request["import"], out ImportNo);

            if ((Request["sort"] != null) && (Request["sort"] != String.Empty))
            {
                Sort.SortByAdd = false;
                Sort.SortByMod = false;
                Sort.SortByDist = false;
                if (Request["sort"] == "add") Sort.SortByAdd = true;
                if (Request["sort"] == "mod") Sort.SortByMod = true;
                if (Request["sort"] == "dist") Sort.SortByDist = true;
            };
            if ((Request["slat"] != null) && (Request["slat"] != String.Empty)) Sort.Lat = StrToFloat(Request["slat"]);
            if ((Request["slat"] != null) && (Request["slat"] != String.Empty)) Sort.Lon = StrToFloat(Request["slat"]);


            if ((Request["tagsallin"] != null) && (Request["tagsallin"] != String.Empty)) TagsAllIn = Request["tagsallin"] == "1";
            if ((Request["tags"] != null) && (Request["tags"] != String.Empty))
            {
                List<string> tlist = new List<string>();
                string[] tags = Request["tags"].Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string tag in tags)
                    tlist.Add(tag.Trim().ToLower());
                Tags = tlist.ToArray();
            };

            if ((Request["nlike"] != null) && (Request["nlike"] != String.Empty)) NameLike = HttpUtility.HtmlEncode(Request["nlike"]);
        }

        #region double <--> string
        private System.Globalization.NumberFormatInfo ni = (System.Globalization.NumberFormatInfo)(System.Globalization.CultureInfo.InstalledUICulture).NumberFormat.Clone();
        public string FloatToStr(double val)
        {
            ni.NumberDecimalSeparator = ".";
            return Convert.ToString(val, ni);
        }
        public double StrToFloat(string val)
        {
            ni.NumberDecimalSeparator = ".";
            return double.Parse(val, ni);
        }
        #endregion

        /// <summary>
        ///     Returns OrderBy Block For T-SQL String
        /// </summary>
        public string OrderBy
        {
            get
            {
                string dop = "";
                if ((Sort.SortByDist) && (Sort.Lat != double.NaN) && (Sort.Lon != double.NaN)) dop += ", (ABS(LAT - " + FloatToStr(Sort.Lat) + ") + ABS(LON - " + FloatToStr(Sort.Lon) + "))";
                if ((Sort.SortByDist) && ((Sort.Lat == double.NaN) || (Sort.Lon != double.NaN))) Sort.SortByAdd = true;
                if (Sort.SortByMod) dop += ", Moderated Desc";
                if (Sort.SortByAdd) dop += ", Created Desc";
                return " " + "ORDER BY FirstOrderBy ASC" + dop + " ";
            }
        }

        /// <summary>
        ///     Returns SQLWhere Block For T-SQL String
        /// </summary>
        public string SqlWhere
        {
            get
            {
                string wherestatus = "((POI_MAIN.ID IN (-1)) ";
                if (View.Hidden) wherestatus += "OR (STATUS = 0 AND Author = " + UID.ToString() + ") ";
                if (View.Public) wherestatus += "OR (STATUS = 2) ";
                if (View.CorrectRequested) wherestatus += "OR (STATUS = 4) ";
                if (!IsManager)
                {
                    if (View.Premoderated) wherestatus += "OR (STATUS = 1 AND Author = " + UID.ToString() + ") ";
                    if (View.Failed) wherestatus += "OR (STATUS = 3 AND Author = " + UID.ToString() + ") ";
                    if (View.MarkedToDelete) wherestatus += "OR (STATUS = 5 AND Author = " + UID.ToString() + ") ";
                    if (View.Deleted) wherestatus += "OR (STATUS = 6 AND Author = " + UID.ToString() + ") ";
                }
                else
                {
                    if (View.Premoderated) wherestatus += "OR (STATUS = 1) ";
                    if (View.Failed) wherestatus += "OR (STATUS = 3) ";
                    if (View.MarkedToDelete) wherestatus += "OR (STATUS = 5) ";
                    if (View.Deleted) wherestatus += "OR (STATUS = 6) ";
                };
                wherestatus += ") ";
                string cats = "";
                foreach (int cat in CatIN)
                {
                    if (cats.Length > 0) cats += ",";
                    cats += cat.ToString();
                };
                string tags = "";
                if (Tags.Length > 0)
                {
                    if (!TagsAllIn)
                    {
                        tags += "AND (POI_MAIN.ID IN (SELECT POIID FROM TAGS WHERE WordID IN (SELECT DISTINCT ID FROM WORDS WHERE WORD IN (";
                        for (int i = 0; i < Tags.Length; i++)
                        {
                            if (i > 0) tags += ",";
                            tags += "'" + Tags[i].ToLower() + "'";
                        };
                        tags += ")))) ";
                    }
                    else
                    {                        
                        for (int i = 0; i < Tags.Length; i++)
                            tags += "AND (POI_MAIN.ID IN (SELECT POIID FROM TAGS WHERE WordID IN (SELECT DISTINCT ID FROM WORDS WHERE WORD = '" + Tags[i].ToLower() + "')))";
                    };
                };
                if (NameLike.Length > 0)
                {

                };
                return " WHERE " + wherestatus +
                    // BOUNDS
                    "AND ((LAT >= " + FloatToStr(Bounds.FLat) + ") AND (LAT <= " + FloatToStr(Bounds.TLat) + ") AND (LON >= " + FloatToStr(Bounds.FLon) + ") AND (LON <= " + FloatToStr(Bounds.TLon) + " )) " +
                    // Author = {0}
                    (AuthorOnly > -1 ? "AND (Author = " + AuthorOnly.ToString() + ") " : "") +
                    // ModifyPOI = {0}
                    (ModifyPOI > -1 ? "AND (ModifyPOI = " + ModifyPOI.ToString() + ") " : "") +
                    // POI = {0}
                    (POIID > 0 ? "AND (POI_MAIN.ID = " + POIID.ToString() + ") " : "") +
                    // CatCount = {0}
                    (CatCount > -1 ? "AND (CatCount = " + CatCount.ToString() + ") " : "") +
                    // Created IN {0,1}
                    "AND ((Created >= CONVERT(datetime,'" + Created.From.ToString("yyyy-MM-dd HH:mm:ss") + "',120)) AND (Created <= CONVERT(datetime,'" + Created.To.ToString("yyyy-MM-dd HH:mm:ss") + "',120))) " +
                    // Moderated IN {0,1} OR NOT
                    "AND (((Moderated >= CONVERT(datetime,'" + Moderated.From.ToString("yyyy-MM-dd HH:mm:ss") + "',120)) AND (Moderated <= CONVERT(datetime,'" + Moderated.To.ToString("yyyy-MM-dd HH:mm:ss") + "',120))) " + (SelectNotModerated ? " OR (Moderated IS NULL)" : "") + ") " +
                    // Expires IN {0,1}
                    "AND ((Expires >= CONVERT(datetime,'" + Expires.From.ToString("yyyy-MM-dd HH:mm:ss") + "',120)) AND (Expires <= CONVERT(datetime,'" + Expires.To.ToString("yyyy-MM-dd HH:mm:ss") + "',120))) " +
                    // ImportNo = {0}
                    (ImportNo > -1 ? "AND (ImportNo = " + ImportNo.ToString() + ") " : "") +
                    // Categories
                    (cats.Length > 0 ? "AND (POI_MAIN.ID IN (SELECT POIID FROM POI_CATS WHERE CATID IN (" + cats + "))) " : "") +
                    // Tags
                    (tags.Length > 0 ? tags : "") +
                    // NameLike
                    (NameLike.Length > 0 ? "AND ((POI_MAIN.NAME_RU LIKE '%" + NameLike.Trim() + "%') OR (POI_MAIN.NAME_EN LIKE '%" + NameLike.Trim() + "%')) " : "") +
                    // Custom SQL
                    (CustomSQLWhereAND.Length > 0 ? "AND (" + CustomSQLWhereAND + ") " : "") +
                    " ";
            }
        }

        /// <summary>
        ///     Return SQL To Get Results
        /// </summary>
        public string FullSQL
        {
            get
            {
                return "SELECT * FROM POI_MAIN LEFT JOIN POI_ADDIT ON POI_ADDIT.ID = POI_MAIN.ID " + SqlWhere + OrderBy;
            }
        }

        /// <summary>
        ///     Returns SQL To Count Results
        /// </summary>
        public string FullSQLCount
        {
            get
            {
                return "SELECT COUNT(*) FROM POI_MAIN  " + SqlWhere;
            }
        }
    }

    public class POIMain : XSerializable<POIMain>
    {
        public class POIAddit
        {
            /// <summary>
            ///     Author Comment
            /// </summary>
            public string AuthorComment = "";
            /// <summary>
            ///     Source Info, Creation Info
            /// </summary>
            public string Source = "";
            /// <summary>
            ///     Contacts Phone
            /// </summary>
            public string Phone = "";
            /// <summary>
            ///     Contacts Fax
            /// </summary>
            public string Fax = "";
            /// <summary>
            ///     Contacts E-mail
            /// </summary>
            public string Email = "";
            /// <summary>
            ///     Contacts Web URL
            /// </summary>
            public string Url = "";
            /// <summary>
            ///     Contacts Else..
            /// </summary>
            public string Contacts = "";
            /// <summary>
            ///     Moder Comment 4 Author
            /// </summary>
            public string ModerText2User = "";
            /// <summary>
            ///     Moder Internal Comment
            /// </summary>
            public string ModerInternalComment = "";
            /// <summary>
            ///     POI Address
            /// </summary>
            public string Address = "";
        }

        public class POIStatus
        {
            private POIMain poi = null;
            public POIStatus(POIMain poi)
            {
                this.poi = poi;
            }

            public bool Hidden { get { return poi.Status == 0; } set { poi.Status = value == true ? 0 : 1; } }
            public bool Premoderated { get { return poi.Status == 1; } set { poi.Status = value == true ? 1 : 0; } }
            public bool Public { get { return poi.Status == 2; } set { poi.Status = value == true ? 2 : 3; } }
            public bool Failed { get { return poi.Status == 3; } set { poi.Status = value == true ? 3 : 2; } }
            public bool CorrectRequested { get { return poi.Status == 4; } set { poi.Status = value == true ? 4 : 3; } }
            public bool MarkedToDelete { get { return poi.Status == 5; } set { poi.Status = value == true ? 5 : 1; } }
            public bool Deleted { get { return poi.Status == 6; } set { poi.Status = value == true ? 6 : 5; } }
        }

        /// <summary>
        ///     POI ID
        /// </summary>
        public int ID = -1;
        /// <summary>
        ///     Latitude
        /// </summary>
        public double Lat = 0;
        /// <summary>
        ///     Longitude
        /// </summary>
        public double Lon = 0;
        /// <summary>
        ///     Status 
        ///     0 - Hidden
        ///     1 - Ready
        ///     2 - Moderated OK
        ///     3 - Failed
        ///     4 - Corrected
        ///     5 - Marked To Delete
        ///     6 - Deleted
        /// </summary>
        public int Status = 0;
        public POIStatus State;
        /// <summary>
        ///     POI Author
        /// </summary>
        public int Author = 0;
        /// <summary>
        ///     Created/Modified
        /// </summary>
        public DateTime Created = DateTime.Now;
        /// <summary>
        ///     Moderated
        /// </summary>
        public DateTime Moderated = DateTime.MinValue;
        /// <summary>
        ///     Expires
        /// </summary>
        public DateTime Expires = DateTime.Now.AddYears(10);
        /// <summary>
        ///     POI Name
        /// </summary>
        public string Name_RU = "";
        /// <summary>
        ///     POI Name
        /// </summary>
        public string Name_EN = "";
        /// <summary>
        ///     POI Garmin Categories Count
        /// </summary>
        public int CatCount = 0;        
        /// <summary>
        ///     POI Description
        /// </summary>
        public string Description = "";
        /// <summary>
        ///     if Corrected POI, ModifyPOI - master POI
        /// </summary>
        public int ModifyPOI = 0;
        /// <summary>
        ///     Import Number
        /// </summary>
        public int ImportNo = 0;
        /// <summary>
        ///     FOB No
        /// </summary>
        public int FirstOrderBy = 100;

        /// <summary>
        ///     POI Garmin Categories
        /// </summary>
        public int[] Categories = new int[0];
        /// <summary>
        ///     POI Tags
        /// </summary>
        public string[] Tags = new string[0];

        /// <summary>
        ///     Additional POI Info
        /// </summary>
        public POIAddit Addit = new POIAddit();    
        /// <summary>
        ///     Custom Parameters
        /// </summary>
        public Hashtable CustomParams = new Hashtable();

        /// <summary>
        ///     Is it Moderator?
        /// </summary>
        public bool IsModerator = false;

        #region constructors
        public POIMain() { this.State = new POIStatus(this); }

        public POIMain(int Author)
        {
            this.State = new POIStatus(this);
            this.Author = Author;
        }

        public POIMain(System.Data.SqlClient.SqlConnection sq, System.Data.SqlClient.SqlDataReader dr, bool additFormDataReader, bool additFromQuery, bool POICatFromQuery, bool CustomParamsFromQuery, bool TagsFromQuery)
        {
            this.State = new POIStatus(this);

            ID = Convert.ToInt32(dr["ID"]);
            Lat = Convert.ToDouble(dr["Lat"]);
            Lon = Convert.ToDouble(dr["Lon"]);
            Status = Convert.ToInt32(dr["Status"]);
            Author = Convert.ToInt32(dr["Author"]);
            Created = Convert.ToDateTime(dr["Created"]);
            if (dr["Moderated"] == DBNull.Value)
                Moderated = DateTime.MinValue;
            else
                Moderated = Convert.ToDateTime(dr["Moderated"]);
            Expires = Convert.ToDateTime(dr["Expires"]);
            Name_RU = NNull(dr["Name_Ru"]);
            Name_EN = NNull(dr["Name_EN"]);
            CatCount = Convert.ToInt32(dr["CatCount"]);
            Description = NNull(dr["Description"]);
            ModifyPOI = Convert.ToInt32(dr["ModifyPOI"]);
            ImportNo = Convert.ToInt32(dr["ImportNo"]);
            FirstOrderBy = Convert.ToInt32(dr["FirstOrderBy"]);

            if (additFormDataReader) ReadAddit(dr);
            if (additFromQuery) ReadAddit(sq);           
            if (POICatFromQuery) ReadPOICAT(sq);
            if (CustomParamsFromQuery) ReadCustomParams(sq);
            if (TagsFromQuery) ReadTags(sq);
        }

        public POIMain(HttpRequest Request, bool readModifyPOI)
        {
            this.State = new POIStatus(this);

            POISQLWhere sw = new POISQLWhere(0);
            if ((Request["id"] != null) && (Request["id"] != String.Empty)) int.TryParse(Request["id"], out ID);
            if ((Request["lat"] != null) && (Request["lat"] != String.Empty)) Lat = sw.StrToFloat(Request["lat"]);
            if ((Request["lon"] != null) && (Request["lon"] != String.Empty)) Lon = sw.StrToFloat(Request["lon"]);
            if ((Request["status"] != null) && (Request["status"] != String.Empty)) int.TryParse(Request["status"], out Status);
            if ((Request["author"] != null) && (Request["author"] != String.Empty)) int.TryParse(Request["author"], out Author);
            if ((Request["exp"] != null) && (Request["exp"] != String.Empty)) Convert.ToDateTime(Request["exp"]);
            if ((Request["nru"] != null) && (Request["nru"] != String.Empty)) Name_RU = Prepare(HttpUtility.HtmlEncode(Request["nru"]));
            if ((Request["nen"] != null) && (Request["nen"] != String.Empty)) Name_EN = Prepare(HttpUtility.HtmlEncode(Request["nen"]));
            if ((Request["cats"] != null) && (Request["cats"] != String.Empty))
            {
                string[] cats = Request["cats"].Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                List<int> clist = new List<int>();
                foreach (string cat in cats)
                {
                    int c = 0;
                    if (int.TryParse(cat, out c)) clist.Add(c);
                };
                CatCount = clist.Count;
            };
            if ((Request["desc"] != null) && (Request["desc"] != String.Empty)) Description = Prepare(HttpUtility.HtmlEncode(Request["desc"]));

            if (readModifyPOI)
            {
                if ((Request["mpoi"] != null) && (Request["mpoi"] != String.Empty)) int.TryParse(Request["mpoi"], out ModifyPOI);
            };
            if ((Request["ford"] != null) && (Request["ford"] != String.Empty)) int.TryParse(Request["ford"], out FirstOrderBy);

            if ((Request["a_ac"] != null) && (Request["a_ac"] != String.Empty)) Addit.AuthorComment = Prepare(HttpUtility.HtmlEncode(Request["a_ac"]));
            if ((Request["a_sr"] != null) && (Request["a_sr"] != String.Empty)) Addit.Source = Prepare(HttpUtility.HtmlEncode(Request["a_sr"]));
            if ((Request["a_ph"] != null) && (Request["a_ph"] != String.Empty)) Addit.Phone = Prepare(HttpUtility.HtmlEncode(Request["a_ph"]));
            if ((Request["a_fx"] != null) && (Request["a_fx"] != String.Empty)) Addit.Fax = Prepare(HttpUtility.HtmlEncode(Request["a_fx"]));
            if ((Request["a_em"] != null) && (Request["a_em"] != String.Empty)) Addit.Email = Prepare(HttpUtility.HtmlEncode(Request["a_em"]));
            if ((Request["a_ur"] != null) && (Request["a_ur"] != String.Empty)) Addit.Url = Prepare(HttpUtility.HtmlEncode(Request["a_ur"]));
            if ((Request["a_co"] != null) && (Request["a_co"] != String.Empty)) Addit.Contacts = Prepare(HttpUtility.HtmlEncode(Request["a_co"]));
            if ((Request["a_mu"] != null) && (Request["a_mu"] != String.Empty)) Addit.ModerText2User = Prepare(HttpUtility.HtmlEncode(Request["a_mu"]));
            if ((Request["a_mi"] != null) && (Request["a_mi"] != String.Empty)) Addit.ModerInternalComment = Prepare(HttpUtility.HtmlEncode(Request["a_mi"]));
            if ((Request["a_ad"] != null) && (Request["a_ad"] != String.Empty)) Addit.Address = Prepare(HttpUtility.HtmlEncode(Request["a_ad"]));

            if ((Request["tags"] != null) && (Request["tags"] != String.Empty))
            {
                string[] tags = Request["tags"].Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                List<string> tlist = new List<string>();
                foreach (string tag in tags)
                    tlist.Add(tag.Trim().ToLower());
                Tags = tlist.ToArray();
            };
        }

        public POIMain(string xmlText)
        {
            this.State = new POIStatus(this);

            XmlDocument xd = new XmlDocument();
            xd.LoadXml(xmlText);

            ID = Convert.ToInt32(xd.SelectSingleNode("poi/id").ChildNodes[0].Value);
            Lat = StrToFloat(xd.SelectSingleNode("poi/lat").ChildNodes[0].Value);
            Lon = StrToFloat(xd.SelectSingleNode("poi/lon").ChildNodes[0].Value);
            Status = Convert.ToInt32(xd.SelectSingleNode("poi/status").ChildNodes[0].Value);
            Author = Convert.ToInt32(xd.SelectSingleNode("poi/author").ChildNodes[0].Value);
            Expires = Convert.ToDateTime(xd.SelectSingleNode("poi/exp").ChildNodes[0].Value);
            Name_RU = Prepare(HttpUtility.HtmlEncode(xd.SelectSingleNode("poi/nru").ChildNodes[0].Value));
            Name_EN = Prepare(HttpUtility.HtmlEncode(xd.SelectSingleNode("poi/nen").ChildNodes[0].Value));

            if (xd.SelectSingleNode("poi/cats").ChildNodes.Count > 0)
            {
                string[] cats = xd.SelectSingleNode("poi/cats").ChildNodes[0].Value.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                List<int> clist = new List<int>();
                foreach (string cat in cats)
                {
                    int c = 0;
                    if (int.TryParse(cat, out c)) clist.Add(c);
                };
                CatCount = clist.Count;
            };

            Addit.AuthorComment = xd.SelectSingleNode("poi/addit/a_ac").ChildNodes.Count > 0 ? Prepare(HttpUtility.HtmlEncode(xd.SelectSingleNode("poi/addit/a_ac").ChildNodes[0].Value)) : "";
            Addit.Source = xd.SelectSingleNode("poi/addit/a_sr").ChildNodes.Count > 0 ? Prepare(HttpUtility.HtmlEncode(xd.SelectSingleNode("poi/addit/a_sr").ChildNodes[0].Value)) : "";
            Addit.Phone = xd.SelectSingleNode("poi/addit/a_ph").ChildNodes.Count > 0 ? Prepare(HttpUtility.HtmlEncode(xd.SelectSingleNode("poi/addit/a_ph").ChildNodes[0].Value)) : "";
            Addit.Fax = xd.SelectSingleNode("poi/addit/a_fx").ChildNodes.Count > 0 ? Prepare(HttpUtility.HtmlEncode(xd.SelectSingleNode("poi/addit/a_fx").ChildNodes[0].Value)) : "";
            Addit.Email = xd.SelectSingleNode("poi/addit/a_em").ChildNodes.Count > 0 ? Prepare(HttpUtility.HtmlEncode(xd.SelectSingleNode("poi/addit/a_em").ChildNodes[0].Value)) : "";
            Addit.Url = xd.SelectSingleNode("poi/addit/a_ur").ChildNodes.Count > 0 ? Prepare(HttpUtility.HtmlEncode(xd.SelectSingleNode("poi/addit/a_ur").ChildNodes[0].Value)) : "";
            Addit.Contacts = xd.SelectSingleNode("poi/addit/a_aco").ChildNodes.Count > 0 ? Prepare(HttpUtility.HtmlEncode(xd.SelectSingleNode("poi/addit/a_co").ChildNodes[0].Value)) : "";
            Addit.ModerText2User = xd.SelectSingleNode("poi/addit/a_mu").ChildNodes.Count > 0 ? Prepare(HttpUtility.HtmlEncode(xd.SelectSingleNode("poi/addit/a_mu").ChildNodes[0].Value)) : "";
            Addit.ModerInternalComment = xd.SelectSingleNode("poi/addit/a_mi").ChildNodes.Count > 0 ? Prepare(HttpUtility.HtmlEncode(xd.SelectSingleNode("poi/addit/a_mi").ChildNodes[0].Value)) : "";
            Addit.Address = xd.SelectSingleNode("poi/addit/a_ad").ChildNodes.Count > 0 ? Prepare(HttpUtility.HtmlEncode(xd.SelectSingleNode("poi/addit/a_ad").ChildNodes[0].Value)) : "";

            if (xd.SelectSingleNode("poi/tags").ChildNodes.Count > 0)
            {
                string[] tags = xd.SelectSingleNode("poi/tags").ChildNodes[0].Value.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                List<string> tlist = new List<string>();
                foreach (string tag in tags)
                    tlist.Add(tag.Trim().ToLower());
                Tags = tlist.ToArray();
            };

            foreach (XmlNode xn in xd.SelectSingleNode("poi/custom/param"))
                CustomParams.Add(xn.SelectSingleNode("name").ChildNodes[0].Value, xn.SelectSingleNode("value").ChildNodes.Count > 0 ? xn.SelectSingleNode("value").ChildNodes[0].Value : "");
        }
        #endregion

        public bool Save(System.Data.SqlClient.SqlConnection sq, int CurrentUserUID)
        {
            bool NEW = false;
            if (sq.State == ConnectionState.Closed) sq.Open();
            SqlCommand sc = new SqlCommand("", sq);
            if (ID < 1)
            {
                NEW = true;
                ID = CurrentUserUID;
                sc.CommandText = "BEGIN TRANSACTION \n " + Get_SQLInsert() + " \n SELECT @@IDENTITY;";
                SqlDataReader dr = sc.ExecuteReader();
                if (dr.Read()) this.ID = Convert.ToInt32(dr[0]);
                dr.Close();
            }
            else
            {
                sc.CommandText = Get_SQLUpdate();
            };
            int rowsA = sc.ExecuteNonQuery();
            if (rowsA > 0)
            {
                WriteAddit(sq);
                WritePOICAT(sq);
                WriteCustomParams(sq);
                WriteTags(sq);
            };

            if (NEW)
            {
                SaveToHistory(sq, Author, true, false, false, false);
                if (IsModerator) SaveToHistory(sq, Author, true, false, false, true);
            }
            else
            {
                if (!IsModerator)
                    SaveToHistory(sq, Author, false, ModifyPOI > 0 ? false : true, ModifyPOI > 0 ? true : false, false);
                else
                    SaveToHistory(sq, CurrentUserUID, false, ModifyPOI > 0 ? false : true, ModifyPOI > 0 ? true : false, true);
            };

            return rowsA > 0;
        }

        public void SaveToHistory(System.Data.SqlClient.SqlConnection sq, int UID, bool addNew, bool update, bool addCorrected, bool moderate)
        {
            if (ID < 1) return;

            string opText = "";
            if (addNew) opText = "Добавление POI";
            if (update) opText = "Изменение POI";
            if (addCorrected) opText = "Корректировка POI";
            int opCode = moderate ? 1 : 0;

            if (sq.State == ConnectionState.Closed) sq.Open();
            SqlCommand sc = new SqlCommand("", sq);
            sc.CommandText = "INSERT INTO HISTORY (UID,POIID,Operation,OpCode) VALUES "+
                "("+UID.ToString()+","+ID.ToString()+",'"+opText+"',"+opCode.ToString()+")";
            sc.ExecuteScalar();
        }

        private void ReadAddit(System.Data.SqlClient.SqlDataReader dr)
        {
            Addit = new POIAddit();
            Addit.AuthorComment = NNull(dr["AuthorComment"]);
            Addit.Source = NNull(dr["Source"]);
            Addit.Phone = NNull(dr["Phone"]);
            Addit.Fax = NNull(dr["Fax"]);
            Addit.Email = NNull(dr["Email"]);
            Addit.Url = NNull(dr["Url"]);
            Addit.Contacts = NNull(dr["Contacts"]);
            Addit.ModerText2User = NNull(dr["ModerText2User"]);
            Addit.ModerInternalComment = NNull(dr["ModerInternalComment"]);
            Addit.Address = NNull(dr["Address"]);
        }

        public void ReadPOICAT(System.Data.SqlClient.SqlConnection sq)
        {
            if (sq.State == System.Data.ConnectionState.Closed) sq.Open();
            System.Data.SqlClient.SqlCommand sc = new System.Data.SqlClient.SqlCommand("SELECT DISTINCT CATID FROM POI_CATS WHERE POIID = " + ID.ToString(), sq);
            System.Data.SqlClient.SqlDataReader dr = sc.ExecuteReader();
            List<int> clist = new List<int>();
            while (dr.Read())
                clist.Add(Convert.ToInt32(dr[0]));
            dr.Close();
            Categories = clist.ToArray();
            CatCount = Categories.Length;
        }

        public void WritePOICAT(System.Data.SqlClient.SqlConnection sq)
        {
            if (sq.State == System.Data.ConnectionState.Closed) sq.Open();
            System.Data.SqlClient.SqlCommand sc = new System.Data.SqlClient.SqlCommand("DELETE FROM POI_CATS WHERE POIID = " + ID.ToString(), sq);
            sc.ExecuteScalar();
            sc.CommandText = "";
            foreach (int cat in Categories)
                sc.CommandText += "INSERT INTO POI_CATS (POIID,CATID) VALUES (" + ID.ToString() + "," + cat.ToString() + "); \n";
            if (sc.CommandText.Length > 0)
            {
                sc.ExecuteScalar();
                sc.CommandText = "UPDATE POI_MAIN SET CatCount = " + Categories.Length.ToString() + " WHERE ID = " + ID.ToString();
            };
            CatCount = Categories.Length;
        }

        public void ReadCustomParams(System.Data.SqlClient.SqlConnection sq)
        {
            CustomParams.Clear();
            if (sq.State == System.Data.ConnectionState.Closed) sq.Open();
            System.Data.SqlClient.SqlCommand sc = new System.Data.SqlClient.SqlCommand("SELECT * FROM POI_CUSTOM WHERE ID = " + ID.ToString(), sq);
            System.Data.SqlClient.SqlDataReader dr = sc.ExecuteReader();
            while (dr.Read())
                CustomParams.Add(NNull(dr["FieldName"]), NNull(dr["FieldValue"]));
            dr.Close();
        }

        public void WriteCustomParams(System.Data.SqlClient.SqlConnection sq)
        {
            if (sq.State == System.Data.ConnectionState.Closed) sq.Open();
            System.Data.SqlClient.SqlCommand sc = new System.Data.SqlClient.SqlCommand("DELETE FROM POI_CUSTOM WHERE ID = " + ID.ToString(), sq);
            sc.ExecuteScalar();
            sc.CommandText = "";
            foreach (DictionaryEntry en in CustomParams)
                sc.CommandText += "INSERT INTO POI_CUSTOM (ID,FieldName,FieldValue) VALUES (" + ID.ToString() + ",'" + en.Key + "','" + en.Value + "'); \n";
            if (sc.CommandText.Length > 0)
                sc.ExecuteScalar();
        }

        public void ReadTags(System.Data.SqlClient.SqlConnection sq)
        {
            if (sq.State == System.Data.ConnectionState.Closed) sq.Open();
            System.Data.SqlClient.SqlCommand sc = new System.Data.SqlClient.SqlCommand("SELECT DISTINCT Word FROM WORDS WHERE ID IN (SELECT WordID FROM TAGS WHERE POIID = " + ID.ToString()+")", sq);
            System.Data.SqlClient.SqlDataReader dr = sc.ExecuteReader();
            List<string> tlist = new List<string>();
            while (dr.Read())
                tlist.Add(NNull(dr[0]));
            dr.Close();
            Tags = tlist.ToArray();
        }

        public void WriteTags(System.Data.SqlClient.SqlConnection sq)
        {
            if (sq.State == System.Data.ConnectionState.Closed) sq.Open();
            System.Data.SqlClient.SqlCommand sc = new System.Data.SqlClient.SqlCommand("DELETE FROM TAGS WHERE POIID = " + ID.ToString(), sq);
            sc.ExecuteScalar();
            foreach (string s in Tags)
            {
                sc.CommandText = "SELECT * FROM WORDS WHERE WORD = '"+s.ToLower()+"'";
                SqlDataReader dr = sc.ExecuteReader();
                if (dr.Read())
                {
                    int id = Convert.ToInt32(dr["ID"]);
                    dr.Close();
                    sc.CommandText = "INSERT INTO TAGS (POIID,WordID) VALUES (" + ID.ToString() + "," + id.ToString() + ")";
                    sc.ExecuteScalar();
                }
                else
                {
                    dr.Close();
                    sc.CommandText = "SELECT MAX(ID)+1 FROM WORDS";
                    dr = sc.ExecuteReader();
                    dr.Read();
                    int id = Convert.ToInt32(dr[0]);
                    dr.Close();
                    sc.CommandText = "INSERT INTO WORDS (ID,Word) VALUES ("+id.ToString()+",'"+s.ToLower()+"'); INSERT INTO TAGS (POIID,WordID) VALUES (" + ID.ToString() + "," + id.ToString() + ")";
                    sc.ExecuteScalar();
                };
            };
        }

        public void ReadAddit(System.Data.SqlClient.SqlConnection sq)
        {
            Addit = new POIAddit();
            if (sq.State == ConnectionState.Closed) sq.Open();
            SqlCommand sc = new SqlCommand("SELECT * FROM POI_ADDIT WHERE ID = " + ID.ToString(), sq);
            SqlDataReader dr2 = sc.ExecuteReader();
            while (dr2.Read())
            {
                Addit.AuthorComment = NNull(dr2["AuthorComment"]);
                Addit.Source = NNull(dr2["Source"]);
                Addit.Phone = NNull(dr2["Phone"]);
                Addit.Fax = NNull(dr2["Fax"]);
                Addit.Email = NNull(dr2["Email"]);
                Addit.Url = NNull(dr2["Url"]);
                Addit.Contacts = NNull(dr2["Contacts"]);
                Addit.ModerText2User = NNull(dr2["ModerText2User"]);
                Addit.ModerInternalComment = NNull(dr2["ModerInternalComment"]);
                Addit.Address = NNull(dr2["Address"]);
            };
            dr2.Close();
        }

        public void WriteAddit(System.Data.SqlClient.SqlConnection sq)
        {
            if (sq.State == System.Data.ConnectionState.Closed) sq.Open();
            (new SqlCommand(Get_SQLInsertAddin(), sq)).ExecuteScalar();
        }

        public string Get_SQLInsert()
        {
            POISQLWhere sw = new POISQLWhere(0);
            string sql = "INSERT INTO POI_MAIN " +
                "(Lat,Lon,Status,Author," +
                "Expires,Name_RU,Name_EN," +
                "Description,ModifyPOI,ImportNo) " +
                "VALUES " +
                "(" + sw.FloatToStr(Lat) + "," + sw.FloatToStr(Lon) + "," + 
                (IsModerator ? Status.ToString() : (Status < 2 ? Status.ToString() : "2")) + 
                "," + Author.ToString() + "," +
                "CONVERT(datetime,'" + Expires.ToString("yyyy-MM-dd HH:mm:ss") + "',120),'" + Name_RU + "','" + Name_EN + "'," +
                "'" + Description + "'," + ModifyPOI.ToString() + "," + ImportNo.ToString() + ")";
            return sql;
        }

        public string Get_SQLInsertAddin()
        {
            POISQLWhere sw = new POISQLWhere(0);
            string sql = "DELETE FROM POI_ADDIT WHERE ID = " + ID.ToString() + ";\n";
            sql += "INSERT INTO POI_ADDIT " +
                "(ID,AuthorComment,Source,Phone,Fax,Email,Url,Contacts,ModerText2User,ModerInternalComment,Address) " +
                "VALUES " +
                "(" + ID.ToString() + ",'" + Addit.AuthorComment + "','" + Addit.Source + "','" + Addit.Phone + "','" + Addit.Fax + "','" + Addit.Email + "','" + Addit.Url + "','" + Addit.Contacts + "','" + Addit.ModerText2User + "','" + Addit.ModerInternalComment + "','" + Addit.Address + "')";
            return sql;
        }

        public string Get_SQLUpdate()
        {
            POISQLWhere sw = new POISQLWhere(0);
            string sql = "UPDATE POI_MAIN " +
                "SET Created = getdate(), Lat = " + sw.FloatToStr(Lat) + ", Lon = " + sw.FloatToStr(Lon) + ", " +
                (IsModerator ? "Status = " + Status.ToString() + ", Author = " + Author.ToString() +", " : "Status = " + (Status < 2 ? Status.ToString() : "2") + ", ") +
                (IsModerator ? "Moderated = getdate(), " : "") +
                "Expires = CONVERT(datetime,'" + Expires.ToString("yyyy-MM-dd HH:mm:ss") + "',120), " +
                "Name_RU = '" + Name_RU + "', Name_EN = '" + Name_EN + "', CatCount = " + CatCount.ToString() + ", " +
                "Description = '" + Description + "', ModifyPOI = " + ModifyPOI.ToString() + ", ImportNo = " + ImportNo.ToString() + ", " +
                "FirstOrderBy = " + FirstOrderBy.ToString() + " " +
                "WHERE ID = " + ID.ToString() +
                (IsModerator ? "" : " AND Author = " + Author.ToString()) +
                ";";
            return sql;
        }

        /// <summary>
        ///     No copy ID, Author, State, ModifyPOI, IsModerator, ImportNo, FirstOrderBy
        /// </summary>
        /// <param name="correctedPOI">Правильно откорректированные данные POI</param>
        public void CopyFromCorrected(POIMain correctedPOI)
        {
            this.Lat = correctedPOI.Lat;
            this.Lon = correctedPOI.Lon;
            this.Created = DateTime.Now;
            if (IsModerator)
                this.Moderated = DateTime.Now;
            this.Expires = correctedPOI.Expires;
            this.Name_RU = correctedPOI.Name_RU;
            this.Name_EN = correctedPOI.Name_EN;
            this.CatCount = correctedPOI.CatCount;
            this.Description = correctedPOI.Description;
            this.Categories = correctedPOI.Categories;
            this.Tags = correctedPOI.Tags;
            this.Addit = correctedPOI.Addit;
            this.CustomParams = correctedPOI.CustomParams;
        }

        /// <summary>
        ///     No Copy ID, Author, State, IsModerator, ImportNo, FirstOrderBy
        /// </summary>
        /// <param name="originalPOI"></param>
        public void CopyFromOriginal(POIMain originalPOI)
        {
            CopyFromCorrected(originalPOI);
            this.ModifyPOI = originalPOI.ID;
        }

        #region static methods
        public static string NNull(object obj)
        {
            return obj == DBNull.Value ? "" : obj.ToString();
        }
        public static bool SNull(string val)
        {
            if (val == null) return true;
            if (val == String.Empty) return true;
            if (val.Length == 0) return true;
            return false;
        }
        public static string Prepare(string val)
        {
            return val.Replace("&", "^");
        }
        #endregion
    }

    public class HTTPR : XSerializable<HTTPR>
    {
        [XSerializable(XML = "ID", Description = "Идентификатор POI")]
        public int POI = 0;
        [XSerializable(XML = "Author", DBField = "Owner", Description = "Автор")]
        public int Owner = 478;
        [XSerializable(XML = "Categories", Description = "Категории")]
        public int[] Cats = new int[2] { 1, 3 };
    }

    [AttributeUsage(AttributeTargets.Field)]
    public class XSerializableAttribute : Attribute
    {        
        public XSerializableAttribute() {}
        
        protected bool no = false;
        /// <summary>
        ///     Ignore by All Serializers
        /// </summary>
        public bool Ignore { get { return this.no; } set { this.no = value; } }

        protected bool nojson = false;
        /// <summary>
        ///     Ignore by JSON Serializer
        /// </summary>
        public bool NoJSON { get { return this.nojson; } set { this.nojson = value; } }

        protected bool noXML = false;
        /// <summary>
        ///     Ignore by XML Serializer
        /// </summary>
        public bool NoXML { get { return this.noXML; } set { this.noXML = value; } }

        protected bool noDBField = false;
        /// <summary>
        ///     Ignore by SqlDataReader Serializer
        /// </summary>
        public bool NoDBField { get { return this.noDBField; } set { this.noDBField = value; } }

        protected bool noRequest = false;
        /// <summary>
        ///     Ignore by HTTPRequest Serializer
        /// </summary>
        public bool NoHTTPRequest { get { return this.noRequest; } set { this.noRequest = value; } }
        
        protected string _XMLName = String.Empty;
        /// <summary>
        ///     XML Serializer Node Name
        /// </summary>
        public string XML { get { return this._XMLName; } set { this._XMLName = value; } }

        protected string _JSONName = String.Empty;
        /// <summary>
        ///     JSON Serializer Parameter Name
        /// </summary>
        public string JSON { get { return this._JSONName; } set { this._JSONName = value; } }

        protected string _RequestName = String.Empty;
        /// <summary>
        ///     HTTPRequest Serializer Parameter Name
        /// </summary>
        public string HTTPRequest { get { return this._RequestName; } set { this._RequestName = value; } }

        protected string _DBDBFieldName = String.Empty;
        /// <summary>
        ///     SQLDataReader Serializer Field Name
        /// </summary>
        public string DBField { get { return this._DBDBFieldName; } set { this._DBDBFieldName = value; } }

        protected string _description = String.Empty;
        /// <summary>
        ///     Description For Parameter
        /// </summary>
        public string Description { get { return this._description; } set { this._description = value; } }
    }    

    public abstract class XSerializable<T>
    {
        /// <summary>
        ///     Serialize To Type
        /// </summary>
        public enum ConvertType
        {
            ctJSON = 0,
            ctXML = 1
        }

        private static string PrepareString(string obj)
        {
            return obj.Replace("\"", "\\\"");
        }

        private static string GetJSO(object obj, ConvertType ct)
        {
            if ((obj.GetType() == typeof(String)) || (obj.GetType() == typeof(Char)))
            {
                string res = "";
                if (ct != ConvertType.ctXML) res += "\"";
                res += PrepareString(obj.ToString());
                if (ct != ConvertType.ctXML) res += "\"";
                return res;
            };
            if (obj.GetType() == typeof(Hashtable))
            {
                string res = "";
                int count = 0;
                foreach (DictionaryEntry en in ((Hashtable)obj))
                {
                    if ((ct != ConvertType.ctXML) && (count > 0))
                        res += ", ";
                    res += "\"" + en.Key + "\" : " + GetJSO(en.Value, ct);
                    count++;
                };
                if (ct == ConvertType.ctJSON) res = "{" + res + "}";
                return res;
            };
            if (obj.GetType() == typeof(Boolean)) return (bool)obj ? "true" : "false";
            if (obj.GetType() == typeof(Double)) return obj.ToString().Replace(",", ".");
            if (obj.GetType() == typeof(Single)) return obj.ToString().Replace(",", ".");
            try { if (((Array)obj) != null) return (ct == ConvertType.ctXML ? "\n" : "") + ArrayToString((Array)obj, ct); }
            catch { };
            try { if (((Enum)obj) != null) return ((int)obj).ToString(); }
            catch { };
            if (obj.GetType() == typeof(DateTime))
            {
                string res = "";
                if (ct != ConvertType.ctXML) res += "\"";
                res += PrepareString(((DateTime)obj).ToString());
                if (ct != ConvertType.ctXML) res += "\"";
                return res;
            };
            return obj.ToString();
        }

        private static string ArrayToString(Array arr, ConvertType ct)
        {
            string res = "";
            foreach (object obj in arr)
            {
                if (ct != ConvertType.ctXML)
                {
                    if (res.Length > 0) res += ", ";
                };
                if (ct == ConvertType.ctXML) res += "<item>";
                res += GetJSO(obj, ct);
                if (ct == ConvertType.ctXML) res += "</item>\n";
            };
            if (ct == ConvertType.ctJSON) res = "[" + res + "]";
            return res;
        }

        /// <summary>
        ///    Serialize Object 2 JSON
        /// </summary>
        /// <returns></returns>
        public static string ToJSON(T obj)
        {
            string res = "{";
            Type t = obj.GetType();
            System.Reflection.FieldInfo[] fieldInfo = t.GetFields();
            foreach (System.Reflection.FieldInfo info in fieldInfo)
            {
                bool isIgnored = false;
                string name = String.Empty;
                foreach (Attribute attr in info.GetCustomAttributes(true))
                {
                    XSerializableAttribute jsa = (XSerializableAttribute)attr;
                    if (null != jsa)
                    {
                        isIgnored = jsa.Ignore || jsa.NoJSON;
                        name = jsa.JSON;
                    };
                };

                if (!isIgnored)
                {
                    if (res.Length > 1) res += ", ";
                    res += "\"" + (((name != null) && (name != String.Empty)) ? name : info.Name) + "\" : " + GetJSO(info.GetValue(obj), ConvertType.ctJSON);
                };
            };
            res += "}";
            return res.Length > 2 ? res : "";
        }

        /// <summary>
        ///     Serialize Object 2 XML
        /// </summary>
        /// <returns></returns>
        public static string ToXML(string root, T obj)
        {
            string res = "";
            Type t = obj.GetType();
            if ((root != null) && (root != String.Empty) && (root.Length > 0)) res += "<" + root + ">\n";
            System.Reflection.FieldInfo[] fieldInfo = t.GetFields();
            foreach (System.Reflection.FieldInfo info in fieldInfo)
            {
                bool isIgnored = false;
                string name = String.Empty;
                foreach (Attribute attr in info.GetCustomAttributes(true))
                {
                    XSerializableAttribute jsa = (XSerializableAttribute)attr;
                    if (null != jsa)
                    {
                        isIgnored = jsa.Ignore || jsa.NoXML;
                        name = jsa.XML;
                    };
                };

                if (!isIgnored)
                {
                    string nm = (((name != null) && (name != String.Empty)) ? name : info.Name);
                    res += "<" + nm + ">" + GetJSO(info.GetValue(obj), ConvertType.ctXML) + "</" + nm + ">\n";
                };
            };
            if ((root != null) && (root != String.Empty) && (root.Length > 0)) res += "</" + root + ">\n";
            return res.Length > 0 ? res : "";
        }

        /// <summary>
        ///     Serialize Object
        /// </summary>
        /// <param name="ct"></param>
        /// <returns></returns>
        public static string ToString(ConvertType ct, T obj)
        {
            if (ct == ConvertType.ctJSON) return ToJSON(obj);
            if (ct == ConvertType.ctXML) return ToXML(typeof(T).Name, obj);
            return "";
        }

        // ParseSimple - string/int/bool/double/int[]/string[]/double[]
        private static void FromDB(ref T classobj, object value, System.Reflection.FieldInfo info, string KeyName)
        {
            object obj = info.GetValue(classobj);
            if ((obj.GetType() == typeof(String)) || (obj.GetType() == typeof(Char)))
                info.SetValue(classobj, value.ToString());
            if (obj.GetType() == typeof(int))
            {
                try
                {
                    info.SetValue(classobj, Convert.ToInt32(value));
                }
                catch (Exception ex) { throw new XSException("Неверный формат числа: `" + value + "` для переменной " + KeyName + ".", ex, KeyName, value.ToString()); }
            };
            if (obj.GetType() == typeof(DateTime))
            {
                try
                {
                    info.SetValue(classobj, Convert.ToDateTime(value));
                }
                catch (Exception ex) { throw new XSException("Неверный формат даты: `" + value + "` для переменной " + KeyName + ".", ex, KeyName, value.ToString()); }
            };
            if (obj.GetType() == typeof(Boolean))
                try
                {
                    info.SetValue(classobj, Convert.ToBoolean(value));
                }
                catch (Exception ex) { throw new XSException("Неверный формат: `" + value + "` для переменной " + KeyName + ".", ex, KeyName, value.ToString()); }
            if (obj.GetType() == typeof(Double))
            {
                try
                {
                    info.SetValue(classobj, Convert.ToDouble(value));
                }
                catch (Exception ex) { throw new XSException("Неверный формат числа: `" + value + "` для переменной " + KeyName + ".", ex, KeyName, value.ToString()); }
            };
            if (obj.GetType() == typeof(Single))
            {
                try
                {
                    info.SetValue(classobj, Convert.ToSingle(value));
                }
                catch (Exception ex) { throw new XSException("Неверный формат числа: `" + value + "` для переменной " + KeyName + ".", ex, KeyName, value.ToString()); }
            };
        }
        /// <summary>
        ///     Load Object From DB
        /// </summary>
        /// <param name="dr"></param>
        /// <returns></returns>
        public static T FromDB(SqlDataReader dr)
        {
            Type asmType = typeof(T);
            System.Reflection.ConstructorInfo ci = asmType.GetConstructor(new Type[] { });
            T res = (T)ci.Invoke(new object[] { });

            System.Reflection.FieldInfo[] fieldInfo = asmType.GetFields();
            foreach (System.Reflection.FieldInfo info in fieldInfo)
            {
                bool isIgnored = false;
                string name = String.Empty;
                string desc = String.Empty;
                foreach (Attribute attr in info.GetCustomAttributes(true))
                {
                    XSerializableAttribute jsa = (XSerializableAttribute)attr;
                    if (null != jsa)
                    {
                        isIgnored = jsa.Ignore || jsa.NoDBField;
                        name = jsa.DBField;
                        desc = jsa.Description;
                    };
                };

                if (!isIgnored)
                {           
                    name = (name != null) && (name != String.Empty) ? name : info.Name;
                    if (dr[name] != null)
                        FromDB(ref res, dr[name], info, desc);
                };
            };
            return res;
        }

        // ParseSimple - string/int/bool/double/int[]/string[]/double[]
        private static void FromHTTPRequest(ref T classobj, string value, System.Reflection.FieldInfo info, string KeyName)
        {
            object obj = info.GetValue(classobj);
            if ((obj.GetType() == typeof(String)) || (obj.GetType() == typeof(Char)))
                info.SetValue(classobj, value);
            if (obj.GetType() == typeof(int))
            {
                try
                {
                    info.SetValue(classobj, Convert.ToInt32(value));
                }
                catch (Exception ex) { throw new XSException("Неверный формат числа: `" + value + "` для переменной " + KeyName + ".", ex, KeyName, value.ToString()); }
            };
            if (obj.GetType() == typeof(DateTime))
            {
                try
                {
                    info.SetValue(classobj, Convert.ToDateTime(value));
                }
                catch (Exception ex) { throw new XSException("Неверный формат даты: `" + value + "` для переменной " + KeyName + ".", ex, KeyName, value.ToString()); }
            };
            if (obj.GetType() == typeof(Boolean))
                info.SetValue(classobj, Convert.ToBoolean(value == "true"));
            if (obj.GetType() == typeof(Double))
            {
                try
                {
                    info.SetValue(classobj, StrToFloat(value));
                }
                catch (Exception ex) { throw new XSException("Неверный формат числа: `" + value + "` для переменной " + KeyName + ".", ex, KeyName, value.ToString()); }
            };
            if (obj.GetType() == typeof(Single))
            {
                try
                {
                    info.SetValue(classobj, StrToFloat(value));
                }
                catch (Exception ex) { throw new XSException("Неверный формат числа: `" + value + "` для переменной " + KeyName + ".", ex, KeyName, value.ToString()); }
            };
            try
            {
                if (((Array)obj) != null)
                {
                    if (obj.GetType() == typeof(int[]))
                    {
                        List<int> lint = new List<int>();
                        string[] inpt = value.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                        foreach (string xnd in inpt)
                            lint.Add(Convert.ToInt32(xnd.Trim()));
                        info.SetValue(classobj, lint.ToArray());
                    };
                    if (obj.GetType() == typeof(string[]))
                    {
                        List<string> lint = new List<string>();
                        string[] inpt = value.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                        foreach (string xnd in inpt)
                            lint.Add(xnd.Trim());
                        info.SetValue(classobj, lint.ToArray());
                    };
                    if (obj.GetType() == typeof(double[]))
                    {
                        List<double> lint = new List<double>();
                        string[] inpt = value.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                        foreach (string xnd in inpt)
                            lint.Add(StrToFloat(xnd.Trim()));
                        info.SetValue(classobj, lint.ToArray());
                    };
                };
            }
            catch { };
        }
        /// <summary>
        ///     Load Object From HTTPRequest
        /// </summary>
        /// <param name="Request"></param>
        /// <returns></returns>
        public static T FromHTTPRequest(HttpRequest Request)
        {
            Type asmType = typeof(T);
            System.Reflection.ConstructorInfo ci = asmType.GetConstructor(new Type[] { });
            T res = (T)ci.Invoke(new object[] { });

            System.Reflection.FieldInfo[] fieldInfo = asmType.GetFields();
            foreach (System.Reflection.FieldInfo info in fieldInfo)
            {
                bool isIgnored = false;
                string name = String.Empty;
                string desc = String.Empty;
                foreach (Attribute attr in info.GetCustomAttributes(true))
                {
                    XSerializableAttribute jsa = (XSerializableAttribute)attr;
                    if (null != jsa)
                    {
                        isIgnored = jsa.Ignore || jsa.NoHTTPRequest;
                        name = jsa.HTTPRequest;
                        desc = jsa.Description;
                    };
                };

                if (!isIgnored)
                {
                    name = (name != null) && (name != String.Empty) ? name : info.Name;
                    if ((Request[name] != null) && (Request[name] != String.Empty))
                        FromHTTPRequest(ref res, Request[name], info, desc);
                };
            };

            return res;
        }

        // ParseSimple - string/int/bool/double/int[]/string[]/double[]
        private static void FromXML(ref T classobj, System.Reflection.FieldInfo info, XmlNode xn, string KeyName)
        {
            object obj = info.GetValue(classobj);
            if ((obj.GetType() == typeof(String)) || (obj.GetType() == typeof(Char)))
                info.SetValue(classobj, xn.ChildNodes.Count > 0 ? xn.ChildNodes[0].Value : "");
            if (obj.GetType() == typeof(int))
            {
                string value = null;
                try
                {
                    value = xn.ChildNodes.Count > 0 ? xn.ChildNodes[0].Value : "";
                    info.SetValue(classobj, Convert.ToInt32(value));
                }
                catch (Exception ex) { throw new XSException("Неверный формат числа `" + value + "` для переменной " + KeyName + ".", ex, KeyName, value); }
            };
            if (obj.GetType() == typeof(DateTime))
            {
                string value = null;
                try
                {
                    value = xn.ChildNodes.Count > 0 ? xn.ChildNodes[0].Value : "";
                    info.SetValue(classobj, Convert.ToDateTime(value));
                }
                catch (Exception ex) { throw new XSException("Неверный формат даты `" + value + "` для переменной " + KeyName + ".", ex, KeyName, value); }
            };
            if (obj.GetType() == typeof(Boolean))
                info.SetValue(classobj, Convert.ToBoolean(xn.ChildNodes.Count > 0 ? xn.ChildNodes[0].Value == "true" : false));
            if (obj.GetType() == typeof(Double))
            {
                string value = null;
                try
                {
                    value = xn.ChildNodes.Count > 0 ? xn.ChildNodes[0].Value : "";
                    info.SetValue(classobj, StrToFloat(value));
                }
                catch (Exception ex) { throw new XSException("Неверный формат числа `" + value + "` для переменной " + KeyName + ".", ex, KeyName, value); }
            };
            if (obj.GetType() == typeof(Single))
            {
                string value = null;
                try
                {
                    value = xn.ChildNodes.Count > 0 ? xn.ChildNodes[0].Value : "";
                    info.SetValue(classobj, StrToFloat(value));
                }
                catch (Exception ex) { throw new XSException("Неверный формат числа `" + value + "` для переменной " + KeyName + ".", ex, KeyName, value); }
            };
            try
            {
                if (((Array)obj) != null)
                {
                    if (obj.GetType() == typeof(int[]))
                    {
                        List<int> lint = new List<int>();
                        foreach (XmlNode xnd in xn.SelectNodes("item"))
                            lint.Add(Convert.ToInt32(xnd.ChildNodes[0].Value));
                        info.SetValue(classobj, lint.ToArray());
                    };
                    if (obj.GetType() == typeof(string[]))
                    {
                        List<string> lint = new List<string>();
                        foreach (XmlNode xnd in xn.SelectNodes("item"))
                            lint.Add(xnd.ChildNodes.Count > 0 ? xnd.ChildNodes[0].Value : "");
                        info.SetValue(classobj, lint.ToArray());
                    };
                    if (obj.GetType() == typeof(double[]))
                    {
                        List<double> lint = new List<double>();
                        foreach (XmlNode xnd in xn.SelectNodes("item"))
                            lint.Add(StrToFloat(xnd.ChildNodes[0].Value));
                        info.SetValue(classobj, lint.ToArray());
                    };
                };
            }
            catch { };
        }
        /// <summary>
        ///     Load Object From XML
        /// </summary>
        /// <param name="root"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public static T FromXML(string root, string data)
        {
            Type asmType = typeof(T);
            System.Reflection.ConstructorInfo ci = asmType.GetConstructor(new Type[] { });
            T res = (T)ci.Invoke(new object[] { });

            bool ok = false;

            System.Xml.XmlDocument xd = new System.Xml.XmlDocument();
            try
            {
                xd.LoadXml(data);
            }
            catch (Exception ex) { throw new XSException("Не удается разобрать XML формат", ex, null, null); };
            string objname = asmType.Name;

            foreach (Attribute attr in asmType.GetCustomAttributes(true))
            {
                XSerializableAttribute jsa = (XSerializableAttribute)attr;
                if (null != jsa)
                {
                    objname = jsa.XML == String.Empty ? objname : jsa.XML;
                };
            };

            System.Reflection.FieldInfo[] fieldInfo = asmType.GetFields();
            foreach (System.Reflection.FieldInfo info in fieldInfo)
            {
                bool isIgnored = false;
                string name = String.Empty;
                string desc = String.Empty;
                foreach (Attribute attr in info.GetCustomAttributes(true))
                {
                    XSerializableAttribute jsa = (XSerializableAttribute)attr;
                    if (null != jsa)
                    {
                        isIgnored = jsa.Ignore || jsa.NoXML;
                        name = jsa.XML;
                        desc = jsa.Description;
                    };
                };

                if (!isIgnored)
                {
                    name = (((name != null) && (name != String.Empty)) ? name : info.Name);
                    XmlNode xn = ((root != null) && (root != String.Empty) && (root.Length > 0)) ? xd.SelectSingleNode(root) : xd;
                    if (xn != null)
                    {
                        xn = xn.SelectSingleNode(name);
                        if (xn != null)
                        {
                            FromXML(ref res, info, xn, desc);
                            ok = true;
                        };
                    };
                };
            };
            if (!ok) throw new XSException("Couldn't create object form XML", null, null, null);
            return res;
        }
        
        /// <summary>
        ///    Serialize Object 2 JSON
        /// </summary>
        /// <returns></returns>
        public string ToJSON()
        {
            string res = "{";
            Type t = this.GetType();
            System.Reflection.FieldInfo[] fieldInfo = t.GetFields();
            foreach (System.Reflection.FieldInfo info in fieldInfo)
            {
                bool isIgnored = false;
                string name = String.Empty;
                foreach (Attribute attr in info.GetCustomAttributes(true))
                {
                    XSerializableAttribute jsa = (XSerializableAttribute)attr;
                    if (null != jsa)
                    {
                        isIgnored = jsa.Ignore || jsa.NoJSON;
                        name = jsa.JSON;
                    };
                };

                if (!isIgnored)
                {
                    if (res.Length > 1) res += ", ";
                    res += "\"" + (((name != null) && (name != String.Empty)) ? name : info.Name) + "\" : " + GetJSO(info.GetValue(this), ConvertType.ctJSON);
                };
            };
            res += "}";
            return res.Length > 2 ? res : "";
        }

        /// <summary>
        ///     Serialize Object 2 XML
        /// </summary>
        /// <returns></returns>
        public string ToXML(string root)
        {
            string res = "";
            Type t = this.GetType();
            if ((root != null) && (root != String.Empty) && (root.Length > 0)) res += "<" + root + ">\n";
            System.Reflection.FieldInfo[] fieldInfo = t.GetFields();
            foreach (System.Reflection.FieldInfo info in fieldInfo)
            {
                bool isIgnored = false;
                string name = String.Empty;
                foreach (Attribute attr in info.GetCustomAttributes(true))
                {
                    XSerializableAttribute jsa = (XSerializableAttribute)attr;
                    if (null != jsa)
                    {
                        isIgnored = jsa.Ignore || jsa.NoXML;
                        name = jsa.XML;
                    };
                };

                if (!isIgnored)
                {
                    string nm = (((name != null) && (name != String.Empty)) ? name : info.Name);
                    res += "<" + nm + ">" + GetJSO(info.GetValue(this), ConvertType.ctXML) + "</" + nm + ">\n";
                };
            };
            if ((root != null) && (root != String.Empty) && (root.Length > 0)) res += "</" + root + ">\n";
            return res.Length > 0 ? res : "";
        }

        /// <summary>
        ///     Serialize Object
        /// </summary>
        /// <param name="ct"></param>
        /// <returns></returns>
        public string ToString(ConvertType ct)
        {
            if (ct == ConvertType.ctJSON) return this.ToJSON();
            if (ct == ConvertType.ctXML) return this.ToXML(this.GetType().Name);
            return "";
        }

        public static string HashTable2JSON(Hashtable ht)
        {
            string res = "{";
            int count = 0;
            foreach (DictionaryEntry en in ht)
            {
                if (count++ > 0) res += ", ";
                res += "\"" + en.Key + "\" : " + GetJSO(en.Value, ConvertType.ctJSON);
            };
            res += "}";
            return res;
        }
        public static Hashtable JSON2HashTable(string data)
        {
            return (Hashtable)JSON.JsonDecode(data);
        }
        public static string HashTable2XML(string root, Hashtable ht)
        {
            string res = "";
            if ((root != null) && (root != String.Empty) && (root.Length > 0)) res += "<" + root + ">";
            int count = 0;
            foreach (DictionaryEntry en in ht)
            {
                if (count++ > 0) res += ", ";
                res += "<" + en.Key + ">" + GetJSO(en.Value, ConvertType.ctXML) + "</" + en.Key + ">";
            };
            if ((root != null) && (root != String.Empty) && (root.Length > 0)) res += "</" + root + ">";
            return res;
        }
        public static Hashtable XML2HashTable(string root, string data)
        {
            Hashtable res = new Hashtable();
            System.Xml.XmlDocument xd = new System.Xml.XmlDocument();
            xd.LoadXml(data);
            xd.SelectSingleNode(root);
            foreach (XmlNode xn in xd.ChildNodes)
                res.Add(xn.Name, xn.ChildNodes.Count > 0 ? xn.ChildNodes[0].Value : "");
            return res;
        }

        private static System.Globalization.NumberFormatInfo ni = (System.Globalization.NumberFormatInfo)(System.Globalization.CultureInfo.InstalledUICulture).NumberFormat.Clone();

        /// <summary>
        ///     Convert double to string
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        public static string FloatToStr(double val)
        {
            ni.NumberDecimalSeparator = ".";
            return Convert.ToString(val, ni);
        }

        /// <summary>
        ///     Convert string to double
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        public static double StrToFloat(string val)
        {
            ni.NumberDecimalSeparator = ".";
            return double.Parse(val, ni);
        }

        public static T CreateCopy(T obj)
        {
            Type asmType = typeof(T);
            System.Reflection.ConstructorInfo ci = asmType.GetConstructor(new Type[] { });
            T res = (T)ci.Invoke(new object[] { });

            System.Reflection.FieldInfo[] fieldInfo = asmType.GetFields();
            foreach (System.Reflection.FieldInfo info in fieldInfo)
                info.SetValue(res,info.GetValue(obj));
            return res;
        }

        public virtual void CopyFrom(T obj)
        {
            Type asmType = typeof(T);
            System.Reflection.ConstructorInfo ci = asmType.GetConstructor(new Type[] { });

            System.Reflection.FieldInfo[] fieldInfo = asmType.GetFields();
            foreach (System.Reflection.FieldInfo info in fieldInfo)
                info.SetValue(this, info.GetValue(obj));
        }
    }

    public class XSException : Exception
    {
        private string _keyName = null;
        private string _keyValue = null;

        public XSException(string message, Exception innerException, string KeyName, string KeyValue)
            : base(message, innerException)
        {
            this._keyName = KeyName;
            this._keyValue = KeyValue;            
        }

        public string KeyName { get { return _keyName; } }
        public string KeyValue { get { return _keyValue; } }
        public string InnerError { get { return (((Exception)InnerException).Message); } }
        public string InnerText { get { return InnerException.ToString(); } }
    }

    class JSON
    {
        public const int TOKEN_NONE = 0;
        public const int TOKEN_CURLY_OPEN = 1;
        public const int TOKEN_CURLY_CLOSE = 2;
        public const int TOKEN_SQUARED_OPEN = 3;
        public const int TOKEN_SQUARED_CLOSE = 4;
        public const int TOKEN_COLON = 5;
        public const int TOKEN_COMMA = 6;
        public const int TOKEN_STRING = 7;
        public const int TOKEN_NUMBER = 8;
        public const int TOKEN_TRUE = 9;
        public const int TOKEN_FALSE = 10;
        public const int TOKEN_NULL = 11;

        private const int BUILDER_CAPACITY = 2000;

        /// <summary>
        /// Parses the string json into a value
        /// </summary>
        /// <param name="json">A JSON string.</param>
        /// <returns>An ArrayList, a Hashtable, a double, a string, null, true, or false</returns>
        public static object JsonDecode(string json)
        {
            bool success = true;

            return JsonDecode(json, ref success);
        }

        /// <summary>
        /// Parses the string json into a value; and fills 'success' with the successfullness of the parse.
        /// </summary>
        /// <param name="json">A JSON string.</param>
        /// <param name="success">Successful parse?</param>
        /// <returns>An ArrayList, a Hashtable, a double, a string, null, true, or false</returns>
        public static object JsonDecode(string json, ref bool success)
        {
            success = true;
            if (json != null)
            {
                char[] charArray = json.ToCharArray();
                int index = 0;
                object value = ParseValue(charArray, ref index, ref success);
                return value;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Converts a Hashtable / ArrayList object into a JSON string
        /// </summary>
        /// <param name="json">A Hashtable / ArrayList</param>
        /// <returns>A JSON encoded string, or null if object 'json' is not serializable</returns>
        public static string JsonEncode(object json)
        {
            StringBuilder builder = new StringBuilder(BUILDER_CAPACITY);
            bool success = SerializeValue(json, builder);
            return (success ? builder.ToString() : null);
        }

        protected static Hashtable ParseObject(char[] json, ref int index, ref bool success)
        {
            Hashtable table = new Hashtable();
            int token;

            // {
            NextToken(json, ref index);

            bool done = false;
            while (!done)
            {
                token = LookAhead(json, index);
                if (token == JSON.TOKEN_NONE)
                {
                    success = false;
                    return null;
                }
                else if (token == JSON.TOKEN_COMMA)
                {
                    NextToken(json, ref index);
                }
                else if (token == JSON.TOKEN_CURLY_CLOSE)
                {
                    NextToken(json, ref index);
                    return table;
                }
                else
                {

                    // name
                    string name = ParseString(json, ref index, ref success);
                    if (!success)
                    {
                        success = false;
                        return null;
                    }

                    // :
                    token = NextToken(json, ref index);
                    if (token != JSON.TOKEN_COLON)
                    {
                        success = false;
                        return null;
                    }

                    // value
                    object value = ParseValue(json, ref index, ref success);
                    if (!success)
                    {
                        success = false;
                        return null;
                    }

                    table[name] = value;
                }
            }

            return table;
        }

        protected static ArrayList ParseArray(char[] json, ref int index, ref bool success)
        {
            ArrayList array = new ArrayList();

            // [
            NextToken(json, ref index);

            bool done = false;
            while (!done)
            {
                int token = LookAhead(json, index);
                if (token == JSON.TOKEN_NONE)
                {
                    success = false;
                    return null;
                }
                else if (token == JSON.TOKEN_COMMA)
                {
                    NextToken(json, ref index);
                }
                else if (token == JSON.TOKEN_SQUARED_CLOSE)
                {
                    NextToken(json, ref index);
                    break;
                }
                else
                {
                    object value = ParseValue(json, ref index, ref success);
                    if (!success)
                    {
                        return null;
                    }

                    array.Add(value);
                }
            }

            return array;
        }

        protected static object ParseValue(char[] json, ref int index, ref bool success)
        {
            switch (LookAhead(json, index))
            {
                case JSON.TOKEN_STRING:
                    return ParseString(json, ref index, ref success);
                case JSON.TOKEN_NUMBER:
                    return ParseNumber(json, ref index);
                case JSON.TOKEN_CURLY_OPEN:
                    return ParseObject(json, ref index, ref success);
                case JSON.TOKEN_SQUARED_OPEN:
                    return ParseArray(json, ref index, ref success);
                case JSON.TOKEN_TRUE:
                    NextToken(json, ref index);
                    return Boolean.Parse("TRUE");
                case JSON.TOKEN_FALSE:
                    NextToken(json, ref index);
                    return Boolean.Parse("FALSE");
                case JSON.TOKEN_NULL:
                    NextToken(json, ref index);
                    return null;
                case JSON.TOKEN_NONE:
                    break;
            }

            success = false;
            return null;
        }

        protected static string ParseString(char[] json, ref int index, ref bool success)
        {
            StringBuilder s = new StringBuilder(BUILDER_CAPACITY);
            char c;

            EatWhitespace(json, ref index);

            // "
            c = json[index++];

            bool complete = false;
            while (!complete)
            {

                if (index == json.Length)
                {
                    break;
                }

                c = json[index++];
                if (c == '"')
                {
                    complete = true;
                    break;
                }
                else if (c == '\\')
                {

                    if (index == json.Length)
                    {
                        break;
                    }
                    c = json[index++];
                    if (c == '"')
                    {
                        s.Append('"');
                    }
                    else if (c == '\\')
                    {
                        s.Append('\\');
                    }
                    else if (c == '/')
                    {
                        s.Append('/');
                    }
                    else if (c == 'b')
                    {
                        s.Append('\b');
                    }
                    else if (c == 'f')
                    {
                        s.Append('\f');
                    }
                    else if (c == 'n')
                    {
                        s.Append('\n');
                    }
                    else if (c == 'r')
                    {
                        s.Append('\r');
                    }
                    else if (c == 't')
                    {
                        s.Append('\t');
                    }
                    else if (c == 'u')
                    {
                        int remainingLength = json.Length - index;
                        if (remainingLength >= 4)
                        {
                            // fetch the next 4 chars
                            char[] unicodeCharArray = new char[4];
                            Array.Copy(json, index, unicodeCharArray, 0, 4);
                            // parse the 32 bit hex into an integer codepoint
                            uint codePoint = UInt32.Parse(new string(unicodeCharArray), NumberStyles.HexNumber);
                            // convert the integer codepoint to a unicode char and add to string
                            s.Append(Char.ConvertFromUtf32((int)codePoint));
                            // skip 4 chars
                            index += 4;
                        }
                        else
                        {
                            break;
                        }
                    }

                }
                else
                {
                    s.Append(c);
                }

            }

            if (!complete)
            {
                success = false;
                return null;
            }

            return s.ToString();
        }

        protected static double ParseNumber(char[] json, ref int index)
        {
            EatWhitespace(json, ref index);

            int lastIndex = GetLastIndexOfNumber(json, index);
            int charLength = (lastIndex - index) + 1;
            char[] numberCharArray = new char[charLength];

            Array.Copy(json, index, numberCharArray, 0, charLength);
            index = lastIndex + 1;
            return Double.Parse(new string(numberCharArray), CultureInfo.InvariantCulture);
        }

        protected static int GetLastIndexOfNumber(char[] json, int index)
        {
            int lastIndex;

            for (lastIndex = index; lastIndex < json.Length; lastIndex++)
            {
                if ("0123456789+-.eE".IndexOf(json[lastIndex]) == -1)
                {
                    break;
                }
            }
            return lastIndex - 1;
        }

        protected static void EatWhitespace(char[] json, ref int index)
        {
            for (; index < json.Length; index++)
            {
                if (" \t\n\r".IndexOf(json[index]) == -1)
                {
                    break;
                }
            }
        }

        protected static int LookAhead(char[] json, int index)
        {
            int saveIndex = index;
            return NextToken(json, ref saveIndex);
        }

        protected static int NextToken(char[] json, ref int index)
        {
            EatWhitespace(json, ref index);

            if (index == json.Length)
            {
                return JSON.TOKEN_NONE;
            }

            char c = json[index];
            index++;
            switch (c)
            {
                case '{':
                    return JSON.TOKEN_CURLY_OPEN;
                case '}':
                    return JSON.TOKEN_CURLY_CLOSE;
                case '[':
                    return JSON.TOKEN_SQUARED_OPEN;
                case ']':
                    return JSON.TOKEN_SQUARED_CLOSE;
                case ',':
                    return JSON.TOKEN_COMMA;
                case '"':
                    return JSON.TOKEN_STRING;
                case '0':
                case '1':
                case '2':
                case '3':
                case '4':
                case '5':
                case '6':
                case '7':
                case '8':
                case '9':
                case '-':
                    return JSON.TOKEN_NUMBER;
                case ':':
                    return JSON.TOKEN_COLON;
            }
            index--;

            int remainingLength = json.Length - index;

            // false
            if (remainingLength >= 5)
            {
                if (json[index] == 'f' &&
                    json[index + 1] == 'a' &&
                    json[index + 2] == 'l' &&
                    json[index + 3] == 's' &&
                    json[index + 4] == 'e')
                {
                    index += 5;
                    return JSON.TOKEN_FALSE;
                }
            }

            // true
            if (remainingLength >= 4)
            {
                if (json[index] == 't' &&
                    json[index + 1] == 'r' &&
                    json[index + 2] == 'u' &&
                    json[index + 3] == 'e')
                {
                    index += 4;
                    return JSON.TOKEN_TRUE;
                }
            }

            // null
            if (remainingLength >= 4)
            {
                if (json[index] == 'n' &&
                    json[index + 1] == 'u' &&
                    json[index + 2] == 'l' &&
                    json[index + 3] == 'l')
                {
                    index += 4;
                    return JSON.TOKEN_NULL;
                }
            }

            return JSON.TOKEN_NONE;
        }

        protected static bool SerializeValue(object value, StringBuilder builder)
        {
            bool success = true;

            if (value is string)
            {
                success = SerializeString((string)value, builder);
            }
            else if (value is Hashtable)
            {
                success = SerializeObject((Hashtable)value, builder);
            }
            else if (value is ArrayList)
            {
                success = SerializeArray((ArrayList)value, builder);
            }
            else if (IsNumeric(value))
            {
                success = SerializeNumber(Convert.ToDouble(value), builder);
            }
            else if ((value is Boolean) && ((Boolean)value == true))
            {
                builder.Append("true");
            }
            else if ((value is Boolean) && ((Boolean)value == false))
            {
                builder.Append("false");
            }
            else if (value == null)
            {
                builder.Append("null");
            }
            else
            {
                success = false;
            }
            return success;
        }

        protected static bool SerializeObject(Hashtable anObject, StringBuilder builder)
        {
            builder.Append("{");

            IDictionaryEnumerator e = anObject.GetEnumerator();
            bool first = true;
            while (e.MoveNext())
            {
                string key = e.Key.ToString();
                object value = e.Value;

                if (!first)
                {
                    builder.Append(", ");
                }

                SerializeString(key, builder);
                builder.Append(":");
                if (!SerializeValue(value, builder))
                {
                    return false;
                }

                first = false;
            }

            builder.Append("}");
            return true;
        }

        protected static bool SerializeArray(ArrayList anArray, StringBuilder builder)
        {
            builder.Append("[");

            bool first = true;
            for (int i = 0; i < anArray.Count; i++)
            {
                object value = anArray[i];

                if (!first)
                {
                    builder.Append(", ");
                }

                if (!SerializeValue(value, builder))
                {
                    return false;
                }

                first = false;
            }

            builder.Append("]");
            return true;
        }

        protected static bool SerializeString(string aString, StringBuilder builder)
        {
            builder.Append("\"");

            char[] charArray = aString.ToCharArray();
            for (int i = 0; i < charArray.Length; i++)
            {
                char c = charArray[i];
                if (c == '"')
                {
                    builder.Append("\\\"");
                }
                else if (c == '\\')
                {
                    builder.Append("\\\\");
                }
                else if (c == '\b')
                {
                    builder.Append("\\b");
                }
                else if (c == '\f')
                {
                    builder.Append("\\f");
                }
                else if (c == '\n')
                {
                    builder.Append("\\n");
                }
                else if (c == '\r')
                {
                    builder.Append("\\r");
                }
                else if (c == '\t')
                {
                    builder.Append("\\t");
                }
                else
                {
                    int codepoint = Convert.ToInt32(c);
                    if ((codepoint >= 32) && (codepoint <= 126))
                    {
                        builder.Append(c);
                    }
                    else
                    {
                        builder.Append("\\u" + Convert.ToString(codepoint, 16).PadLeft(4, '0'));
                    }
                }
            }

            builder.Append("\"");
            return true;
        }

        protected static bool SerializeNumber(double number, StringBuilder builder)
        {
            builder.Append(Convert.ToString(number, CultureInfo.InvariantCulture));
            return true;
        }

        /// <summary>
        /// Determines if a given object is numeric in any way
        /// (can be integer, double, null, etc). 
        /// 
        /// Thanks to mtighe for pointing out Double.TryParse to me.
        /// </summary>
        protected static bool IsNumeric(object o)
        {
            double result;

            return (o == null) ? false : Double.TryParse(o.ToString(), out result);
        }
    }
}
