using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;

namespace I2.Loc
{
    public static partial class LocalizationManager
    {

	    public static readonly string Language_CN = "Chinese (Simplified)";
	    public static readonly string Language_EN = "English";
	    public static readonly string Language_JA = "Japanese";
	    public static readonly string Language_KO = "Korean";
	    
	    public static readonly string ServerLanguage_CN = "cn";
	    public static readonly string ServerLanguage_EN = "en";
	    public static readonly string ServerLanguage_JA = "jp";
	    public static readonly string ServerLanguage_KO = "kr";

        public static Dictionary<string, string> LanguageUIName = new Dictionary<string, string>()
        {
            {Language_CN,"简体中文"},
            {Language_EN,"English"},
            {Language_KO,"한국인"},
            {Language_JA,"日本語"}
        };

        #region Variables: CurrentLanguage

        public static string GetServerLanguage()
        {
	        var lang = CurrentLanguage;
			if (string.IsNullOrEmpty(lang))
				return ServerLanguage_EN;
			if (lang == Language_CN)
			{
				return ServerLanguage_CN;
			}else if (lang == Language_EN)
			{
				return ServerLanguage_EN;
			}else if (lang == Language_JA)
			{
				return ServerLanguage_JA;
			}else if (lang == Language_KO)
			{
				return ServerLanguage_KO;
			}
			return ServerLanguage_EN;
		}
        
        public static string CurrentLanguage
        {
            get {
                InitializeIfNeeded();
                return mCurrentLanguage;
            }
            set {
                InitializeIfNeeded();
                string SupportedLanguage = GetSupportedLanguage(value);
                if (!string.IsNullOrEmpty(SupportedLanguage) && mCurrentLanguage != SupportedLanguage)
                {
                    SetLanguageAndCode(SupportedLanguage, GetLanguageCode(SupportedLanguage));
                }
            }
        }
        public static string CurrentLanguageCode
        {
            get {
                InitializeIfNeeded();
                return mLanguageCode; }
            set {
                InitializeIfNeeded();
                if (mLanguageCode != value)
                {
                    string LanName = GetLanguageFromCode(value);
                    if (!string.IsNullOrEmpty(LanName))
                        SetLanguageAndCode(LanName, value);
                }
            }
        }

        // "English (United States)" (get returns "United States") 
        // when set "Canada", the new language code will be "English (Canada)"
        public static string CurrentRegion
        {
            get {
                var Lan = CurrentLanguage;
                int idx = Lan.IndexOfAny("/\\".ToCharArray());
                if (idx > 0)
                    return Lan.Substring(idx + 1);

                idx = Lan.IndexOfAny("[(".ToCharArray());
                int idx2 = Lan.LastIndexOfAny("])".ToCharArray());
                if (idx > 0 && idx != idx2)
                    return Lan.Substring(idx + 1, idx2 - idx - 1);
                return string.Empty;
            }
            set {
                var Lan = CurrentLanguage;
                int idx = Lan.IndexOfAny("/\\".ToCharArray());
                if (idx > 0)
                {
                    CurrentLanguage = Lan.Substring(idx + 1) + value;
                    return;
                }

                idx = Lan.IndexOfAny("[(".ToCharArray());
                int idx2 = Lan.LastIndexOfAny("])".ToCharArray());
                if (idx > 0 && idx != idx2)
                    Lan = Lan.Substring(idx);

                CurrentLanguage = Lan + "(" + value + ")";
            }
        }

        // "en-US" (get returns "US") (when set "CA", the new language code will be "en-CA")
        public static string CurrentRegionCode
        {
            get {
                var code = CurrentLanguageCode;
                int idx = code.IndexOfAny(" -_/\\".ToCharArray());
                return idx < 0 ? string.Empty : code.Substring(idx + 1);
            }
            set {
                var code = CurrentLanguageCode;
                int idx = code.IndexOfAny(" -_/\\".ToCharArray());
                if (idx > 0)
                    code = code.Substring(0, idx);

                CurrentLanguageCode = code + "-" + value;
            }
        }

        public static CultureInfo CurrentCulture
        {
            get
            {
                return mCurrentCulture;
            }
        }

        static string mCurrentLanguage;
        static string mLanguageCode;
        static CultureInfo mCurrentCulture;
        static bool mChangeCultureInfo;

        public static bool IsRight2Left;
        public static bool HasJoinedWords;  // Some languages (e.g. Chinese, Japanese and Thai) don't add spaces to their words (all characters are placed toguether)

        #endregion

        public static void SetLanguageAndCode(string LanguageName, string LanguageCode, bool RememberLanguage = true, bool Force = false)
        {
            if (mCurrentLanguage != LanguageName || mLanguageCode != LanguageCode || Force)
            {
                if (RememberLanguage)
                    PersistentStorage.SetSetting_String("I2 Language", LanguageName);
                mCurrentLanguage = LanguageName;
                mLanguageCode = LanguageCode;
                mCurrentCulture = CreateCultureForCode(LanguageCode);
                if (mChangeCultureInfo)
                    SetCurrentCultureInfo();

                IsRight2Left = IsRTL(mLanguageCode);
                HasJoinedWords = GoogleLanguages.LanguageCode_HasJoinedWord(mLanguageCode);
                LocalizeAll(Force);
            }
        }

        static CultureInfo CreateCultureForCode(string code)
        {
#if !NETFX_CORE
            try
            {
                return CultureInfo.CreateSpecificCulture(code);
            }
            catch (Exception)
            {
                return CultureInfo.InvariantCulture;
            }
#else
			return CultureInfo.InvariantCulture;
#endif
        }

        public static void EnableChangingCultureInfo(bool bEnable)
        {
            if (!mChangeCultureInfo && bEnable)
                SetCurrentCultureInfo();
            mChangeCultureInfo = bEnable;
        }

        static void SetCurrentCultureInfo()
        {
            #if !NETFX_CORE
                Thread.CurrentThread.CurrentCulture = mCurrentCulture;
            #endif
        }


        static void SelectStartupLanguage()
        {
			if (Sources.Count == 0)
				return;

            // Use the system language if there is a source with that language, 
            // or pick any of the languages provided by the sources

            string SavedLanguage = PersistentStorage.GetSetting_String("I2 Language", string.Empty);
            string SysLanguage = GetCurrentDeviceLanguage();

            // Try selecting the System Language
            // But fallback to the first language found  if the System Language is not available in any source

			if (!string.IsNullOrEmpty(SavedLanguage) && HasLanguage(SavedLanguage, Initialize: false, SkipDisabled:true))
            {
                SetLanguageAndCode(SavedLanguage, GetLanguageCode(SavedLanguage));
                return;
            }

			if (!Sources [0].IgnoreDeviceLanguage) 
			{
				// Check if the device language is supported. 
				// Also recognize when not region is set ("English (United State") will be used if sysLanguage is "English")
				string ValidLanguage = GetSupportedLanguage (SysLanguage, true);
				if (!string.IsNullOrEmpty (ValidLanguage)) {
					SetLanguageAndCode (ValidLanguage, GetLanguageCode (ValidLanguage), false);
					return;
				}
			}

            //--[ Use first language that its not disabled ]-----------
            for (int i = 0, imax = Sources.Count; i < imax; ++i)
                if (Sources[i].mLanguages.Count > 0)
                {
                    for (int j = 0; j < Sources[i].mLanguages.Count; ++j)
                        if (Sources[i].mLanguages[j].IsEnabled())
                        {
                            SetLanguageAndCode(Sources[i].mLanguages[j].Name, Sources[i].mLanguages[j].Code, false);
                            return;
                        }
                }
        }

 
		public static bool HasLanguage( string Language, bool AllowDiscartingRegion = true, bool Initialize=true, bool SkipDisabled=true )
		{
			if (Initialize)
				InitializeIfNeeded();

			// First look for an exact match
			for (int i=0, imax=Sources.Count; i<imax; ++i)
				if (Sources[i].GetLanguageIndex(Language, false, SkipDisabled) >=0)
					return true;

			// Then allow matching "English (Canada)" to "english"
			if (AllowDiscartingRegion)
			{
				for (int i=0, imax=Sources.Count; i<imax; ++i)
					if (Sources[i].GetLanguageIndex(Language, true, SkipDisabled) >=0)
						return true;
			}
			return false;
		}

		// Returns the provided language or a similar one without the Region 
		//(e.g. "English (Canada)" could be mapped to "english" or "English (United States)" if "English (Canada)" is not found
		public static string GetSupportedLanguage( string Language, bool ignoreDisabled=false )
		{
            // First try finding the language that matches one of the official languages
            string code = GoogleLanguages.GetLanguageCode(Language);
            if (!string.IsNullOrEmpty(code))
            {
                // First try finding if the exact language code is in one source
                for (int i = 0, imax = Sources.Count; i < imax; ++i)
                {
                    int Idx = Sources[i].GetLanguageIndexFromCode(code, true, ignoreDisabled);
                    if (Idx >= 0)
                        return Sources[i].mLanguages[Idx].Name;
                }

                // If not, try checking without the region
                for (int i = 0, imax = Sources.Count; i < imax; ++i)
                {
                    int Idx = Sources[i].GetLanguageIndexFromCode(code, false, ignoreDisabled);
                    if (Idx >= 0)
                        return Sources[i].mLanguages[Idx].Name;
                }
            }

            // If not found, then try finding an exact match for the name
            for (int i=0, imax=Sources.Count; i<imax; ++i)
			{
				int Idx = Sources[i].GetLanguageIndex(Language, false, ignoreDisabled);
				if (Idx>=0)
					return Sources[i].mLanguages[Idx].Name;
			}
			
			// Then allow matching "English (Canada)" to "english"
			for (int i=0, imax=Sources.Count; i<imax; ++i)
			{
				int Idx = Sources[i].GetLanguageIndex(Language, true, ignoreDisabled);
				if (Idx>=0)
					return Sources[i].mLanguages[Idx].Name;
			}

			return string.Empty;
		}

		public static string GetLanguageCode( string Language )
		{
			if (Sources.Count==0)
				UpdateSources();
			for (int i=0, imax=Sources.Count; i<imax; ++i)
			{
				int Idx = Sources[i].GetLanguageIndex(Language);
				if (Idx>=0)
					return Sources[i].mLanguages[Idx].Code;
			}
			return string.Empty;
		}

		public static string GetLanguageFromCode( string Code, bool exactMatch=true )
		{
			if (Sources.Count==0)
				UpdateSources();
			for (int i=0, imax=Sources.Count; i<imax; ++i)
			{
				int Idx = Sources[i].GetLanguageIndexFromCode(Code, exactMatch);
				if (Idx>=0)
					return Sources[i].mLanguages[Idx].Name;
			}
			return string.Empty;
		}


		public static List<string> GetAllLanguages ( bool SkipDisabled = true )
		{
			if (Sources.Count==0)
				UpdateSources();
			List<string> Languages = new List<string> ();
			for (int i=0, imax=Sources.Count; i<imax; ++i)
			{
				Languages.AddRange(Sources[i].GetLanguages(SkipDisabled).Where(x=>!Languages.Contains(x)));
			}
			return Languages;
		}

		public static List<string> GetAllLanguagesCode(bool allowRegions=true, bool SkipDisabled = true)
		{
			List<string> Languages = new List<string>();
			for (int i = 0, imax = Sources.Count; i < imax; ++i)
			{
				Languages.AddRange(Sources[i].GetLanguagesCode(allowRegions, SkipDisabled).Where(x => !Languages.Contains(x)));
			}
			return Languages;
		}

		public static bool IsLanguageEnabled(string Language)
		{
			for (int i = 0, imax = Sources.Count; i < imax; ++i)
				if (!Sources[i].IsLanguageEnabled(Language))
					return false;
			return true;
		}

        static void LoadCurrentLanguage()
        {
            for (int i = 0; i < Sources.Count; ++i)
            {
                var iCurrentLang = Sources[i].GetLanguageIndex(mCurrentLanguage, true, false);
                Sources[i].LoadLanguage(iCurrentLang, true, true, true, false);
            }
        }


        // This function should only be called from within the Localize Inspector to temporaly preview that Language
        public static void PreviewLanguage(string NewLanguage)
		{
			mCurrentLanguage = NewLanguage;
			mLanguageCode = GetLanguageCode(mCurrentLanguage);
			IsRight2Left = IsRTL(mLanguageCode);
            HasJoinedWords = GoogleLanguages.LanguageCode_HasJoinedWord(mLanguageCode);
        }
    }
}
