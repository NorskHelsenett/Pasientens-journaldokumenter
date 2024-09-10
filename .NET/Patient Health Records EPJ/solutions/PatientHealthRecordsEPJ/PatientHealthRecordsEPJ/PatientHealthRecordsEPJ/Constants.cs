public class Constants
{
	public const string SessionLoginResultKey = "loginResult";

	public const long KB = 1024;
	public const long MB = KB * 1024;
	public const long GB = MB * 1024;

	// Dette er privatnøkkelen til 2141d734-7604-42ca-a377-27f1b712f4e6 - "Patient Health Records API EPJ"
	public const string JwkPrivateKey = """ 
                {
            "p": "_8WHFd_pMbnVMwNVn1zxuK3SOrTb0yQyq6j3yQ2bTkOGnZ3Qqh760fDcyDrW4h_Hhy2ssbPbuwyNFf9j5boDQ1R3IZL8hFq-dkF55pHGRMSBt0OVQYWu79mfU8KVaeYi439N9Hgt_XRl5J8VOaxN2sx1tWe4Gbjy9z6IEyJ8LgM",
            "kty": "RSA",
            "q": "iRR5niXtUmxH2cUNQCq5WhzMEvpAC5BbgBEyG0KjTX7RflbJmNflb-lwwLSNsZROGSAHra3rYvA5oXfsbdlIbkOmOpV6AeySAwYPjTpp5E2qx4Jnc7v23Fl9hAfeQLJp3Gsg1lFs1COUXZmvzA06mI2Xor5gyHiTC1MOjmu4CZ0",
            "d": "EJ2l3QwmSiehnBHSdagMpk1_TomKLS9aMp91lJHXJpm6NAg_WWml0S41uE1rZE0veQBIeODeLtV9PR4J6S46XanqKiHGT4os_WLJIMD4yZGH05Hbot2jb9rfif5uRYVJ4oEBAaZdTmr0UFDqAn3PCwx2YziW6EOrAeQU5-XLFPlvFhscLJhTonZpDL6soQBMyJUV57a6-6Lsm3iNxbCd-x_DaZSyS4iB0RxBXGwyvWPL8846L6Ch87Tt2PAa1lVRb4YZgCmOiar9oPtRe6GI2lbyNGtVkxi6UXGpGMdeyXRRRhfuGGmdq0z1RWTl_fw32Y6vHNnrLhk66LPha-pgSQ",
            "e": "AQAB",
            "use": "sig",
            "kid": "oIAHPtvFo3z2jc1NV4x8FUOTGkDhyer2o6_9cxWV72o",
            "qi": "30xJ0V11tvtVI1--woxliXKx9aUpNZ3zQ0KgmiAZSvP2Y6bNccLwMnb5I8ctLocRiazeIrvNT7LwB4Kdc_hCyE0TxqC9pWKevMtzwVKReSRmO2Zp7OGty1gzSpd7ZmZNbJGLpCWvKkB34AiVJLvq9Xk9B7xB8oQ0oPMYjfNklA8",
            "dp": "XQ1dBw1ORqQIN6X36aGj43-Bo5AXBar-sEsp0sCbhw60E_XNa4cX03dgq4wUj3HmYnywfnoj79oyHhWrH3HcLAu3x8Q1EGX_MwDBO6w_SNeBJc51p7_eFC7Fc3CwcTWj7cZr8wwiFzrYsyNZUoEoVfjaviO868bIxkC-us9qBEc",
            "alg": "RS256",
            "dq": "OKiN3CAxB3BaAljAMVV3ZxegAfgNoLn6F-UDoODaPp28WUvl55bU7ny-begy6MMzxI7InbDVKf_A0kyPHJhCp9CuVGiUFVeCwl0u8GDEg-jWPcBYoi1-y3TxvDDEXFliCAj_eZYONWC_YjCzyNNu9L8RfeLSfgnYPUYHLja3ysE",
            "n": "iPUqO6EwqOdtCtUIpO40basdlyD18hNc412alGkVKvJ4ieIJZAEiYY7z0FIHVPytKZ-FLD_6Ut10aZVlXSanfIQAxjQ89Z2v0Jk1-toGU_sFq4EeEDQ7zlyk4KvQFsgN5wsoG0hxOwafvTK4mYKsaVVWLoYzvHgB-2YPANPTJE8RQZ6juLWmdt2S0yGW2bRW2kMSf8Rpnaj_ufGFudTtNbLTdVzbhaoMZJ_YnOOyX8b6u5E2hN3ucWxCpnsseEmHcWYP5CyvybrlI6KlQDo4tvweziZ_86S_nbeJJa56CW_DcEA9tdvLmi_q6g2K_Dhe7ndb7uFHrfqYq4AY1u5S1w"
        }
        """;

	public static class Oid
	{
		public const string DirektoratetForEhelse = "2.16.578.1.12.4.1";
		public const string HelseMidtNorgeRhf = "2.16.578.1.12.4.2";
		public const string HelseSorOstRhf = "2.16.578.1.12.4.3";
		public const string HelseNordRhf = "2.16.578.1.12.4.4";
		public const string HelseVestRhf = "2.16.578.1.34";
		public const string NorskHelseNett = "2.16.578.1.12.4.5";

		/// <summary>
		///     2.16.578.1.12.4.1.4
		///     Hver OID under denne noden representerer en identifikasjonsserie for personer, organisasjoner etv
		/// </summary>
		public const string Identifikasjonsserie = "2.16.578.1.12.4.1.4";

		public const string Fodselsnummer = "2.16.578.1.12.4.1.4.1";
		public const string DNummer = "2.16.578.1.12.4.1.4.2";
		public const string FellesHjelpenummer = "2.16.578.1.12.4.1.4.3";
		public const string HprNummer = "2.16.578.1.12.4.1.4.4";
		public const string DufNummer = "2.16.578.1.12.4.1.4.5";
		public const string OrganisasjonNummer = "2.16.578.1.12.4.1.4.101";
		public const string ReshId = "2.16.578.1.12.4.1.4.102";
		public const string ApotekKonsesjonsnummer = "2.16.578.1.12.4.1.4.107";

		/// <summary>
		///     2.16.578.1.12.4.1.2
		///     Identifikator for kommunikasjonsparter i helsenettet.
		///     Norsk Helsenett foretar tildelinger under denne noden
		/// </summary>
		public const string HerId = "2.16.578.1.12.4.1.2";

		/// <summary>
		///     2.16.578.1.12.4.1.6
		///     Hver OID under denne noden representerer en EPJ-komponenttype
		/// </summary>
		public const string EpjKomponentType = "2.16.578.1.12.4.1.6";

		public const string EpjDataelementType = "2.16.578.1.12.4.1.6.11";
		public const string EpjFragmentType = "2.16.578.1.12.4.1.6.12";
		public const string EpjDokumentType = "2.16.578.1.12.4.1.6.13";
		public const string EpjSaksType = "2.16.578.1.12.4.1.6.14";

		/// <summary>
		///     2.16.578.1.12.4.1.1
		///     Kodeverk publisert på volven.no
		/// </summary>
		public const string KodeverkVolven = "2.16.578.1.12.4.1.1";

		public const string AnatomiskLokalisasjon = "2.16.578.1.12.4.1.1.8352";
		public const string NorskLaboratoriekodever = "2.16.578.1.12.4.1.1.7280";
		public const string Provemateriale = "2.16.578.1.12.4.1.1.8351";

		public const string ConfidentialityCode = "2.16.840.1.113883.5.25";
		public const string CdaRootTypeId = "2.16.840.1.113883.1.3";

		public static class MockEhelse
		{
			public const string Helsenorge = DirektoratetForEhelse + ".99";
			public const string HelsenorgeRepTyper = Helsenorge + ".50";
			public const string HelsenorgeRepForelder = HelsenorgeRepTyper + ".1";
			public const string HelsenorgeRepFullmakt = HelsenorgeRepTyper + ".2";
			public const string HelsenorgeRepKilder = Helsenorge + ".70";
			public const string HelsenorgeRepKildeHn = HelsenorgeRepKilder + ".1";
		}
	}
}