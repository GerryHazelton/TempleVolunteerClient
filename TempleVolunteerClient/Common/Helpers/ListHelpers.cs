﻿using Microsoft.AspNetCore.Mvc.Rendering;

namespace TempleVolunteerClient.Common
{
    public static class ListHelpers
    {
        public enum ManageMessage
        {
            AddPhoneSuccess = 1,
            ChangePasswordSuccess,
            SetTwoFactorSuccess,
            SetPasswordSuccess,
            RemoveLoginSuccess,
            RemovePhoneSuccess,
            PersonalInfoSuccess,
            UsernameSuccess,
            Error
        }

        public enum ErrorType
        {
            Create = 1,
            Edit,
            Delete,
            Index
        }

        public enum ContentType
        {
            Doc = 1,
            Docx = 2,
            Pdf = 3,
            Jpg = 4,
            Gif = 5,
            Png = 6
        }

        static public List<SelectListItem> GenderList { get; } = new List<SelectListItem>
        {
            new SelectListItem { Value = "Man", Text = "Man"},
            new SelectListItem { Value = "Woman", Text = "Woman"},
        };

        static public List<SelectListItem> States { get; } = new List<SelectListItem>
        {
            new SelectListItem { Value = "", Text = ""},
            new SelectListItem { Value = "AL", Text = "Alabama"},
            new SelectListItem { Value = "AK", Text = "Alaska"},
            new SelectListItem { Value = "AZ", Text = "Arizona"},
            new SelectListItem { Value = "AR", Text = "Arkansas"},
            new SelectListItem { Value = "CA", Text = "California"},
            new SelectListItem { Value = "CO", Text = "Colorado"},
            new SelectListItem { Value = "CT", Text = "Connecticut"},
            new SelectListItem { Value = "DE", Text = "Delaware"},
            new SelectListItem { Value = "FL", Text = "Florida"},
            new SelectListItem { Value = "GA", Text = "Georgia"},
            new SelectListItem { Value = "HI", Text = "Hawaii"},
            new SelectListItem { Value = "ID", Text = "Idaho"},
            new SelectListItem { Value = "IL", Text = "Illinois"},
            new SelectListItem { Value = "IN", Text = "Indiana"},
            new SelectListItem { Value = "IA", Text = "Iowa"},
            new SelectListItem { Value = "KS", Text = "Kansas"},
            new SelectListItem { Value = "KY", Text = "Kentucky"},
            new SelectListItem { Value = "LA", Text = "Louisiana"},
            new SelectListItem { Value = "ME", Text = "Maine"},
            new SelectListItem { Value = "MD", Text = "Mrayland"},
            new SelectListItem { Value = "MA", Text = "Massachusetts"},
            new SelectListItem { Value = "MI", Text = "Michigan"},
            new SelectListItem { Value = "MN", Text = "Minnesota"},
            new SelectListItem { Value = "MS", Text = "Mississippi"},
            new SelectListItem { Value = "MO", Text = "Missouri"},
            new SelectListItem { Value = "MT", Text = "Montana"},
            new SelectListItem { Value = "NE", Text = "Nebraska"},
            new SelectListItem { Value = "NV", Text = "Nevada"},
            new SelectListItem { Value = "NH", Text = "New Hampshire"},
            new SelectListItem { Value = "NJ", Text = "New Jersey"},
            new SelectListItem { Value = "NM", Text = "New Mexico"},
            new SelectListItem { Value = "NY", Text = "New York"},
            new SelectListItem { Value = "NC", Text = "North Carolina"},
            new SelectListItem { Value = "ND", Text = "North Dakota"},
            new SelectListItem { Value = "OH", Text = "Ohio"},
            new SelectListItem { Value = "OK", Text = "Oklahoma"},
            new SelectListItem { Value = "OR", Text = "Oregon"},
            new SelectListItem { Value = "PA", Text = "Pennsylvania"},
            new SelectListItem { Value = "RI", Text = "Rhode Island"},
            new SelectListItem { Value = "SC", Text = "South Carolina"},
            new SelectListItem { Value = "SD", Text = "South Dakota"},
            new SelectListItem { Value = "TN", Text = "Tennessee"},
            new SelectListItem { Value = "TX", Text = "Texas"},
            new SelectListItem { Value = "UT", Text = "Utah"},
            new SelectListItem { Value = "VT", Text = "Vermont"},
            new SelectListItem { Value = "VA", Text = "Virginia"},
            new SelectListItem { Value = "WA ", Text = "Washington"},
            new SelectListItem { Value = "WV", Text = "West Virginia"},
            new SelectListItem { Value = "WI", Text = "Wisconsin"},
            new SelectListItem { Value = "WY", Text = "Wyoming"},
        };

        static public List<SelectListItem> Countries { get; } = new List<SelectListItem>
        {
            new SelectListItem { Value = "US", Text = "United States"},
            new SelectListItem { Value = "AD", Text = "Andorra"},
            new SelectListItem { Value = "AE", Text = "United Arab Emirates"},
            new SelectListItem { Value = "AF", Text = "Afghanistan"},
            new SelectListItem { Value = "AG", Text = "Antigua"},
            new SelectListItem { Value = "AL", Text = "Albania"},
            new SelectListItem { Value = "AM", Text = "Armenia"},
            new SelectListItem { Value = "AO", Text = "Angola"},
            new SelectListItem { Value = "AR", Text = "Argentina"},
            new SelectListItem { Value = "AT", Text = "Austria"},
            new SelectListItem { Value = "AU", Text = "Australia"},
            new SelectListItem { Value = "AZ", Text = "Azerbaijan"},
            new SelectListItem { Value = "BA", Text = "Bosnia"},
            new SelectListItem { Value = "BB", Text = "Barbados"},
            new SelectListItem { Value = "BD", Text = "Bangladesh"},
            new SelectListItem { Value = "BE", Text = "Belgium"},
            new SelectListItem { Value = "BF", Text = "Burkina"},
            new SelectListItem { Value = "BG", Text = "Bulgaria"},
            new SelectListItem { Value = "BH", Text = "Bahrain"},
            new SelectListItem { Value = "BI", Text = "Burundi"},
            new SelectListItem { Value = "BJ", Text = "Benin"},
            new SelectListItem { Value = "BN", Text = "Brunei"},
            new SelectListItem { Value = "BO", Text = "Bolivia"},
            new SelectListItem { Value = "BR", Text = "Brazil"},
            new SelectListItem { Value = "BS", Text = "Bahamas"},
            new SelectListItem { Value = "BT", Text = "Bhutan"},
            new SelectListItem { Value = "BW", Text = "Botswana"},
            new SelectListItem { Value = "BY", Text = "Belarus"},
            new SelectListItem { Value = "BZ", Text = "Belize"},
            new SelectListItem { Value = "CA", Text = "Canada"},
            new SelectListItem { Value = "CD", Text = "Congo"},
            new SelectListItem { Value = "CF", Text = "Africa"},
            new SelectListItem { Value = "CH", Text = "Switzerland"},
            new SelectListItem { Value = "CI", Text = "Côte d'Ivoire"},
            new SelectListItem { Value = "CL", Text = "Chile"},
            new SelectListItem { Value = "CM", Text = "Cameroon"},
            new SelectListItem { Value = "CN", Text = "China"},
            new SelectListItem { Value = "CO", Text = "Colombia"},
            new SelectListItem { Value = "CR", Text = "Costa Rica"},
            new SelectListItem { Value = "CU", Text = "Cuba"},
            new SelectListItem { Value = "CV", Text = "Cape Ver"},
            new SelectListItem { Value = "CY", Text = "Cyprus"},
            new SelectListItem { Value = "CZ", Text = "Czech Republic"},
            new SelectListItem { Value = "DE", Text = "Germany"},
            new SelectListItem { Value = "DJ", Text = "Djibouti"},
            new SelectListItem { Value = "DK", Text = "Denmark"},
            new SelectListItem { Value = "DM", Text = "Dominica"},
            new SelectListItem { Value = "DO", Text = "Dominican Republic"},
            new SelectListItem { Value = "DZ", Text = "Algeria"},
            new SelectListItem { Value = "EC", Text = "Ecuador"},
            new SelectListItem { Value = "EE", Text = "Estonia"},
            new SelectListItem { Value = "EG", Text = "Egypt"},
            new SelectListItem { Value = "ER", Text = "Eritrea"},
            new SelectListItem { Value = "ES", Text = "Spain"},
            new SelectListItem { Value = "ET", Text = "Ethiopia"},
            new SelectListItem { Value = "FI", Text = "Finland"},
            new SelectListItem { Value = "FJ", Text = "Fiji"},
            new SelectListItem { Value = "FM", Text = "Micronesia"},
            new SelectListItem { Value = "FR", Text = "France"},
            new SelectListItem { Value = "GA", Text = "Gabon"},
            new SelectListItem { Value = "GB", Text = "Great Britain"},
            new SelectListItem { Value = "GD", Text = "Grenada"},
            new SelectListItem { Value = "GE", Text = "Georgia"},
            new SelectListItem { Value = "GH", Text = "Ghana"},
            new SelectListItem { Value = "GM", Text = "Gambia"},
            new SelectListItem { Value = "GN", Text = "Guinea"},
            new SelectListItem { Value = "GQ", Text = "Guinea"},
            new SelectListItem { Value = "GR", Text = "Greece"},
            new SelectListItem { Value = "GT", Text = "Guatemala"},
            new SelectListItem { Value = "GW", Text = "Guinea-Bissau"},
            new SelectListItem { Value = "GY", Text = "Guyana"},
            new SelectListItem { Value = "HN", Text = "Honduras"},
            new SelectListItem { Value = "HR", Text = "Croatia"},
            new SelectListItem { Value = "HT", Text = "Haiti"},
            new SelectListItem { Value = "HU", Text = "Hungary"},
            new SelectListItem { Value = "ID", Text = "Indonesia"},
            new SelectListItem { Value = "IE", Text = "Ireland"},
            new SelectListItem { Value = "IL", Text = "Israel"},
            new SelectListItem { Value = "IN", Text = "India"},
            new SelectListItem { Value = "IQ", Text = "Iraq"},
            new SelectListItem { Value = "IR", Text = "Iran"},
            new SelectListItem { Value = "IS", Text = "Iceland"},
            new SelectListItem { Value = "IT", Text = "Italy"},
            new SelectListItem { Value = "JM", Text = "Jamaica"},
            new SelectListItem { Value = "JO", Text = "Jordan"},
            new SelectListItem { Value = "JP", Text = "Japan"},
            new SelectListItem { Value = "KE", Text = "Kenya"},
            new SelectListItem { Value = "KG", Text = "Kyrgyzstan"},
            new SelectListItem { Value = "KH", Text = "Cambodia"},
            new SelectListItem { Value = "KI", Text = "Kiribati"},
            new SelectListItem { Value = "KM", Text = "Comoros"},
            new SelectListItem { Value = "KP", Text = "Korea"},
            new SelectListItem { Value = "KW", Text = "Kuwait"},
            new SelectListItem { Value = "KZ", Text = "Kazakhstan"},
            new SelectListItem { Value = "LA", Text = "Lao"},
            new SelectListItem { Value = "LB", Text = "Lebanon"},
            new SelectListItem { Value = "LC", Text = "Saint Lucia"},
            new SelectListItem { Value = "LI", Text = "Liechtenstein"},
            new SelectListItem { Value = "LK", Text = "Sri Lanka"},
            new SelectListItem { Value = "LR", Text = "Liberia"},
            new SelectListItem { Value = "LS", Text = "Lesotho"},
            new SelectListItem { Value = "LT", Text = "Lithuania"},
            new SelectListItem { Value = "LU", Text = "Luxembourg"},
            new SelectListItem { Value = "LV", Text = "Latvia"},
            new SelectListItem { Value = "LY", Text = "Libya"},
            new SelectListItem { Value = "MA", Text = "Morocco"},
            new SelectListItem { Value = "MC", Text = "Monaco"},
            new SelectListItem { Value = "MD", Text = "Moldova"},
            new SelectListItem { Value = "ME", Text = "Montenegro"},
            new SelectListItem { Value = "MG", Text = "Madagascar"},
            new SelectListItem { Value = "MH", Text = "Marshall Islands"},
            new SelectListItem { Value = "MK", Text = "Macedonia"},
            new SelectListItem { Value = "ML", Text = "Mali"},
            new SelectListItem { Value = "MM", Text = "Myanmar"},
            new SelectListItem { Value = "MN", Text = "Mongolia"},
            new SelectListItem { Value = "MR", Text = "Mauritania"},
            new SelectListItem { Value = "MT", Text = "Malta"},
            new SelectListItem { Value = "MU", Text = "Mauritius"},
            new SelectListItem { Value = "MV", Text = "Maldives"},
            new SelectListItem { Value = "MW", Text = "Malawi"},
            new SelectListItem { Value = "MX", Text = "Mexico"},
            new SelectListItem { Value = "MY", Text = "Malaysia"},
            new SelectListItem { Value = "MZ", Text = "Mozambique"},
            new SelectListItem { Value = "NA", Text = "Namibia"},
            new SelectListItem { Value = "NE", Text = "Niger"},
            new SelectListItem { Value = "NG", Text = "Nigeria"},
            new SelectListItem { Value = "NI", Text = "Nicaragua"},
            new SelectListItem { Value = "NL", Text = "Netherlands"},
            new SelectListItem { Value = "NO", Text = "Norway"},
            new SelectListItem { Value = "NP", Text = "Nepal"},
            new SelectListItem { Value = "NR", Text = "Nauru"},
            new SelectListItem { Value = "NZ", Text = "New Zealand"},
            new SelectListItem { Value = "OM", Text = "Oman"},
            new SelectListItem { Value = "PA", Text = "Panama"},
            new SelectListItem { Value = "PE", Text = "Peru"},
            new SelectListItem { Value = "PG", Text = "Papua"},
            new SelectListItem { Value = "PH", Text = "Philippines"},
            new SelectListItem { Value = "PK", Text = "Pakistan"},
            new SelectListItem { Value = "PL", Text = "Poland"},
            new SelectListItem { Value = "PT", Text = "Portugal"},
            new SelectListItem { Value = "PW", Text = "Palau"},
            new SelectListItem { Value = "PY", Text = "Paraguay"},
            new SelectListItem { Value = "QA", Text = "Qatar"},
            new SelectListItem { Value = "RO", Text = "Romania"},
            new SelectListItem { Value = "RS", Text = "Serbia"},
            new SelectListItem { Value = "RU", Text = "Russia"},
            new SelectListItem { Value = "RW", Text = "Rwanda"},
            new SelectListItem { Value = "SA", Text = "Saudi Arabia"},
            new SelectListItem { Value = "SB", Text = "Solomon Islands"},
            new SelectListItem { Value = "SC", Text = "Seychelles"},
            new SelectListItem { Value = "SD", Text = "Sudan"},
            new SelectListItem { Value = "SE", Text = "Sweden"},
            new SelectListItem { Value = "SG", Text = "Singapore"},
            new SelectListItem { Value = "SI", Text = "Slovenia"},
            new SelectListItem { Value = "SK", Text = "Slovakia"},
            new SelectListItem { Value = "SL", Text = "Sierra Leone"},
            new SelectListItem { Value = "SM", Text = "San Marino"},
            new SelectListItem { Value = "SN", Text = "Senegal"},
            new SelectListItem { Value = "SO", Text = "Somalia"},
            new SelectListItem { Value = "SR", Text = "Suriname"},
            new SelectListItem { Value = "SS", Text = "South Sudan"},
            new SelectListItem { Value = "ST", Text = "Sao"},
            new SelectListItem { Value = "SV", Text = "El Salvador"},
            new SelectListItem { Value = "SY", Text = "Syria"},
            new SelectListItem { Value = "SZ", Text = "Swaziland"},
            new SelectListItem { Value = "TD", Text = "Chad"},
            new SelectListItem { Value = "TG", Text = "Togo"},
            new SelectListItem { Value = "TH", Text = "Thailand"},
            new SelectListItem { Value = "TJ", Text = "Tajikistan"},
            new SelectListItem { Value = "TL", Text = "Timor"},
            new SelectListItem { Value = "TM", Text = "Turkmenistan"},
            new SelectListItem { Value = "TN", Text = "Tunisia"},
            new SelectListItem { Value = "TO", Text = "Tonga"},
            new SelectListItem { Value = "TR", Text = "Turkey"},
            new SelectListItem { Value = "TT", Text = "Trinidad"},
            new SelectListItem { Value = "TV", Text = "Tuvalu"},
            new SelectListItem { Value = "TZ", Text = "Tanzania"},
            new SelectListItem { Value = "UA", Text = "Ukraine"},
            new SelectListItem { Value = "UG", Text = "Uganda"},
            new SelectListItem { Value = "UY", Text = "Uruguay"},
            new SelectListItem { Value = "UZ", Text = "Uzbekistan"},
            new SelectListItem { Value = "VC", Text = "Saint Vincent"},
            new SelectListItem { Value = "VE", Text = "Venezuela"},
            new SelectListItem { Value = "VN", Text = "Viet Nam"},
            new SelectListItem { Value = "VU", Text = "Vanuatu"},
            new SelectListItem { Value = "WS", Text = "Samoa"},
            new SelectListItem { Value = "YE", Text = "Yemen"},
            new SelectListItem { Value = "ZA", Text = "South Africa"},
            new SelectListItem { Value = "ZM", Text = "Zambia"},
            new SelectListItem { Value = "ZW", Text = "Zimbabwe"},
            new SelectListItem { Value = "MX", Text = "Mexico"},
            new SelectListItem { Value = "CA", Text = "Canada"},
            new SelectListItem { Value = "US", Text = "USA"},
        };
    }
}
