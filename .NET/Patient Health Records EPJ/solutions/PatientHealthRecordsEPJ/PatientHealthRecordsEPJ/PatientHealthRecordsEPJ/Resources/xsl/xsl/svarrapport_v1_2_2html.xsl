<?xml version="1.0" encoding="utf-8"?>
<!-- Made with XMLSpy v2009 sp1 (http://www.altova.com) by Jan Sigurd Dragsjø (KITH AS) -->
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:lsr="http://www.kith.no/xmlstds/labsvar/2005-03-14" xmlns="http://www.w3.org/1999/xhtml" xmlns:xhtml="http://www.w3.org/1999/xhtml" exclude-result-prefixes="lsr xhtml">
    <xsl:import href="../felleskomponenter/funksjoner.xsl"/>
    <xsl:output method="html" version="1.0" encoding="UTF-8" indent="yes" omit-xml-declaration="yes" doctype-public="-//W3C//DTD XHTML 1.0 Strict//EN" doctype-system="http://www.w3.org/TR/xhtml1/DTD/xhtml1-strict.dtd"/>
    <!-- Visningsfil versjon 1.0
    -    Inngår i KITHs visningsfiler versjon 10
    -->
    <!-- Endringslogg
    -    01.12.10: Rettet Avsender og mottaker som var byttet om. Import av felles CSS-fil.
    -    23.04.10: La til visning av interne referanser ved bruk av IdResultItem of RefIdResultItem.
    -    10.07.09: Felles komplett visningsfil for svarrapport.
    -    04.02.09: Oppdatert adresse for namespace for v1.3
    -    23.06.08: Versjon etter QA
    -->
    <!-- Design
    -    Ingen tomme felt så langt det lar seg gjøre
    -    Radene strekkes alltid til full kolonnebredde (bortsett fra enkelte overskrifter) vha formelen:    <td colspan="{(($col)-1-count(<elementer foran>)*2)*number(not(<elementer bak>))+1}">
        -    Om det gjenstår elementer bak i raden ( number(not(<elementer bak>))=0 ) gir formelen:    <td colspan="1"> (multipliserer med 0 slik at bare "1" gjenstår)
        -    Om det ikke er elementer bak beregner formelen - utfra antall elementer foran i raden - antall kolonner som gjenstår til kolonnebredden ($col) er nådd.
        -    Ett-tallene oppveier hverandre
    -    Klikkbar menylinje
    -    Headervisning avhengig av utskriftsmedium
    -    Kommentarer for ofte tildelt fulle rader
    -->
    <!-- Svakheter
    -    Kolonnevariablene er foreløpig ikke dynamiske (forminsker man variablene dannes ikke flere rader)
    -    Fravær av obligatoriske element kan gi tomme bokser/rader
    -    Fravær av DN-attributt (ev. OT-attributt) kan gi tomme bokser/rader
    -    Tabellen med undersøkelsesresultat blir ganske fort lang og stygg om det er mange ulike elementer med, spes. om StructuredInfo er med.
    -->
    <!-- Variabel-deklarasjon -->
    <!-- Variabler for kolonnebredde -->
    <xsl:variable name="structured-col-width" select="'40%'"/>
    <xsl:variable name="structured-head-width" select="'20%'"/>
    <!-- Variabler for tabellenes kolonner -->
    <xsl:variable name="std-col" select="10"/>
    <xsl:variable name="result-col" select="20"/>
    <xsl:variable name="servreport-col" select="$std-col"/>
    <xsl:variable name="servreq-col" select="$std-col"/>
    <xsl:variable name="pasient-col" select="$std-col"/>
    <xsl:variable name="analysed-col" select="$std-col"/>
    <xsl:variable name="hcp-col" select="$std-col"/>
    <xsl:variable name="refdoc-col" select="$std-col"/>
    <!-- Variabler for bruk til referanser i undersøkelsestabellen -->
    <xsl:variable name="und-komm" select="'k'"/>
    <xsl:variable name="und-spes" select="'s'"/>
    <xsl:variable name="res-komm" select="'r'"/>
    <xsl:variable name="dia-komm" select="'d'"/>
    <!-- (hårete) Variabler for beregning av antall kolonner i undersøkelsestabellen -->
    <xsl:variable name="investigation-col" select="(($result-col)-1)*number(not(//lsr:TextResult | //lsr:Interval | //lsr:DateResult | //lsr:NumResult | //lsr:RefInterval  | //lsr:DevResultInd | //lsr:InvDate | //lsr:DescrDate | //lsr:CounterSignDate | //lsr:MedicalValidationDate | //lsr:ResultItem/lsr:Accredited | //lsr:StructuredInfo | //lsr:ResultItem/lsr:RelServProv | //lsr:Investigation/lsr:Comment | //lsr:Investigation/lsr:Spec | //lsr:ResultItem/lsr:Comment | //lsr:ResultItem/lsr:DiagComment))+1"/>
    <xsl:variable name="textresult-col" select="(($result-col)-2-number(boolean(//lsr:Investigation)))*number(not(//lsr:Interval | //lsr:DateResult | //lsr:NumResult | //lsr:RefInterval  | //lsr:DevResultInd | //lsr:InvDate | //lsr:DescrDate | //lsr:CounterSignDate | //lsr:MedicalValidationDate | //lsr:ResultItem/lsr:Accredited | //lsr:StructuredInfo | //lsr:ResultItem/lsr:RelServProv | //lsr:Investigation/lsr:Comment | //lsr:Investigation/lsr:Spec | //lsr:ResultItem/lsr:Comment | //lsr:ResultItem/lsr:DiagComment))+1"/>
    <xsl:variable name="interval-col" select="(($result-col)-2-number(boolean(//lsr:Investigation))-number(boolean(//lsr:TextResult))*2)*number(not(//lsr:DateResult | //lsr:NumResult | //lsr:RefInterval  | //lsr:DevResultInd | //lsr:InvDate | //lsr:DescrDate | //lsr:CounterSignDate | //lsr:MedicalValidationDate | //lsr:ResultItem/lsr:Accredited | //lsr:StructuredInfo | //lsr:ResultItem/lsr:RelServProv | //lsr:Investigation/lsr:Comment | //lsr:Investigation/lsr:Spec | //lsr:ResultItem/lsr:Comment | //lsr:ResultItem/lsr:DiagComment))+1"/>
    <xsl:variable name="dateresult-col" select="(($result-col)-1-number(boolean(//lsr:Investigation))-number(boolean(//lsr:TextResult))*2-number(boolean(//lsr:Interval))*2)*number(not(//lsr:NumResult | //lsr:RefInterval  | //lsr:DevResultInd | //lsr:InvDate | //lsr:DescrDate | //lsr:CounterSignDate | //lsr:MedicalValidationDate | //lsr:ResultItem/lsr:Accredited | //lsr:StructuredInfo | //lsr:ResultItem/lsr:RelServProv | //lsr:Investigation/lsr:Comment | //lsr:Investigation/lsr:Spec | //lsr:ResultItem/lsr:Comment | //lsr:ResultItem/lsr:DiagComment))+1"/>
    <xsl:variable name="numresult-col" select="(($result-col)-2-number(boolean(//lsr:Investigation))-number(boolean(//lsr:TextResult))*2-number(boolean(//lsr:Interval))*2-number(boolean(lsr:DateResult)))*number(not(//lsr:RefInterval  | //lsr:DevResultInd | //lsr:InvDate | //lsr:DescrDate | //lsr:CounterSignDate | //lsr:MedicalValidationDate | //lsr:ResultItem/lsr:Accredited | //lsr:StructuredInfo | //lsr:ResultItem/lsr:RelServProv | //lsr:Investigation/lsr:Comment | //lsr:Investigation/lsr:Spec | //lsr:ResultItem/lsr:Comment | //lsr:ResultItem/lsr:DiagComment))+1"/>
    <xsl:variable name="refinterval-col" select="(($result-col)-1-number(boolean(//lsr:Investigation))-number(boolean(//lsr:TextResult))*2-number(boolean(//lsr:Interval))*2-number(boolean(lsr:DateResult))-number(boolean(//lsr:NumResult))*2)*number(not(//lsr:DevResultInd | //lsr:InvDate | //lsr:DescrDate | //lsr:CounterSignDate | //lsr:MedicalValidationDate | //lsr:ResultItem/lsr:Accredited | //lsr:StructuredInfo | //lsr:ResultItem/lsr:RelServProv | //lsr:Investigation/lsr:Comment | //lsr:Investigation/lsr:Spec | //lsr:ResultItem/lsr:Comment | //lsr:ResultItem/lsr:DiagComment))+1"/>
    <xsl:variable name="devresultind-col" select="(($result-col)-1-number(boolean(//lsr:Investigation))-number(boolean(//lsr:TextResult))*2-number(boolean(//lsr:Interval))*2-number(boolean(lsr:DateResult))-number(boolean(//lsr:NumResult))*2-number(boolean(//lsr:RefInterval)))*number(not(//lsr:InvDate | //lsr:DescrDate | //lsr:CounterSignDate | //lsr:MedicalValidationDate | //lsr:ResultItem/lsr:Accredited | //lsr:StructuredInfo | //lsr:ResultItem/lsr:RelServProv | //lsr:Investigation/lsr:Comment | //lsr:Investigation/lsr:Spec | //lsr:ResultItem/lsr:Comment | //lsr:ResultItem/lsr:DiagComment))+1"/>
    <xsl:variable name="invdate-col" select="(($result-col)-1-number(boolean(//lsr:Investigation))-number(boolean(//lsr:TextResult))*2-number(boolean(//lsr:Interval))*2-number(boolean(lsr:DateResult))-number(boolean(//lsr:NumResult))*2-number(boolean(//lsr:RefInterval))-number(boolean(//lsr:DevResultInd)))*number(not(//lsr:DescrDate | //lsr:CounterSignDate | //lsr:MedicalValidationDate | //lsr:ResultItem/lsr:Accredited | //lsr:StructuredInfo | //lsr:ResultItem/lsr:RelServProv | //lsr:Investigation/lsr:Comment | //lsr:Investigation/lsr:Spec | //lsr:ResultItem/lsr:Comment | //lsr:ResultItem/lsr:DiagComment))+1"/>
    <xsl:variable name="descrdate-col" select="(($result-col)-1-number(boolean(//lsr:Investigation))-number(boolean(//lsr:TextResult))*2-number(boolean(//lsr:Interval))*2-number(boolean(lsr:DateResult))-number(boolean(//lsr:NumResult))*2-number(boolean(//lsr:RefInterval))-number(boolean(//lsr:DevResultInd))-number(boolean(//lsr:InvDate)))*number(not(//lsr:CounterSignDate | //lsr:MedicalValidationDate | //lsr:ResultItem/lsr:Accredited | //lsr:StructuredInfo | //lsr:ResultItem/lsr:RelServProv | //lsr:Investigation/lsr:Comment | //lsr:Investigation/lsr:Spec | //lsr:ResultItem/lsr:Comment | //lsr:ResultItem/lsr:DiagComment))+1"/>
    <xsl:variable name="countersign-col" select="(($result-col)-1-number(boolean(//lsr:Investigation))-number(boolean(//lsr:TextResult))*2-number(boolean(//lsr:Interval))*2-number(boolean(lsr:DateResult))-number(boolean(//lsr:NumResult))*2-number(boolean(//lsr:RefInterval))-number(boolean(//lsr:DevResultInd))-number(boolean(//lsr:InvDate))-number(boolean(//lsr:DescrDate)))*number(not(//lsr:MedicalValidationDate | //lsr:ResultItem/lsr:Accredited | //lsr:StructuredInfo | //lsr:ResultItem/lsr:RelServProv | //lsr:Investigation/lsr:Comment | //lsr:Investigation/lsr:Spec | //lsr:ResultItem/lsr:Comment | //lsr:ResultItem/lsr:DiagComment))+1"/>
    <xsl:variable name="medvaldate-col" select="(($result-col)-1-number(boolean(//lsr:Investigation))-number(boolean(//lsr:TextResult))*2-number(boolean(//lsr:Interval))*2-number(boolean(lsr:DateResult))-number(boolean(//lsr:NumResult))*2-number(boolean(//lsr:RefInterval))-number(boolean(//lsr:DevResultInd))-number(boolean(//lsr:InvDate))-number(boolean(//lsr:DescrDate))-number(boolean(//lsr:CounterSignDate)))*number(not(//lsr:ResultItem/lsr:Accredited | //lsr:StructuredInfo | //lsr:ResultItem/lsr:RelServProv | //lsr:Investigation/lsr:Comment | //lsr:Investigation/lsr:Spec | //lsr:ResultItem/lsr:Comment | //lsr:ResultItem/lsr:DiagComment))+1"/>
    <xsl:variable name="accred-col" select="(($result-col)-1-number(boolean(//lsr:Investigation))-number(boolean(//lsr:TextResult))*2-number(boolean(//lsr:Interval))*2-number(boolean(lsr:DateResult))-number(boolean(//lsr:NumResult))*2-number(boolean(//lsr:RefInterval))-number(boolean(//lsr:DevResultInd))-number(boolean(//lsr:InvDate))-number(boolean(//lsr:DescrDate))-number(boolean(//lsr:CounterSignDate))-number(boolean(//lsr:MedicalValidationDate)))*number(not(//lsr:StructuredInfo | //lsr:ResultItem/lsr:RelServProv | //lsr:Investigation/lsr:Comment | //lsr:Investigation/lsr:Spec | //lsr:ResultItem/lsr:Comment | //lsr:ResultItem/lsr:DiagComment))+1"/>
    <xsl:variable name="structured-col" select="(($result-col)-1-number(boolean(//lsr:Investigation))-number(boolean(//lsr:TextResult))*2-number(boolean(//lsr:Interval))*2-number(boolean(lsr:DateResult))-number(boolean(//lsr:NumResult))*2-number(boolean(//lsr:RefInterval))-number(boolean(//lsr:DevResultInd))-number(boolean(//lsr:InvDate))-number(boolean(//lsr:DescrDate))-number(boolean(//lsr:CounterSignDate))-number(boolean(//lsr:MedicalValidationDate))-number(boolean(//lsr:ResultItem/lsr:Accredited)))*number(not(//lsr:ResultItem/lsr:RelServProv | //lsr:Investigation/lsr:Comment | //lsr:Investigation/lsr:Spec | //lsr:ResultItem/lsr:Comment | //lsr:ResultItem/lsr:DiagComment))+1"/>
    <xsl:variable name="relserv-col" select="(($result-col)-1-number(boolean(//lsr:Investigation))-number(boolean(//lsr:TextResult))*2-number(boolean(//lsr:Interval))*2-number(boolean(lsr:DateResult))-number(boolean(//lsr:NumResult))*2-number(boolean(//lsr:RefInterval))-number(boolean(//lsr:DevResultInd))-number(boolean(//lsr:InvDate))-number(boolean(//lsr:DescrDate))-number(boolean(//lsr:CounterSignDate))-number(boolean(//lsr:MedicalValidationDate))-number(boolean(//lsr:ResultItem/lsr:Accredited))-number(boolean(//lsr:ResultItem/lsr:StructuredInfo)))*number(not(//lsr:Investigation/lsr:Comment | //lsr:Investigation/lsr:Spec | //lsr:ResultItem/lsr:Comment | //lsr:ResultItem/lsr:DiagComment))+1"/>
    <xsl:variable name="merknad-col" select="($result-col)-number(boolean(//lsr:Investigation))-number(boolean(//lsr:TextResult))*2-number(boolean(//lsr:Interval))*2-number(boolean(lsr:DateResult))-number(boolean(//lsr:NumResult))*2-number(boolean(//lsr:RefInterval))-number(boolean(//lsr:DevResultInd))-number(boolean(//lsr:InvDate))-number(boolean(//lsr:DescrDate))-number(boolean(//lsr:CounterSignDate))-number(boolean(//lsr:MedicalValidationDate))-number(boolean(//lsr:ResultItem/lsr:Accredited))-number(boolean(//lsr:ResultItem/lsr:StructuredInfo))-number(boolean(//lsr:ResultItem/lsr:RelServProv))"/>
    <!-- Meldingsstart -->
    <xsl:template match="/">
        <html xmlns="http://www.w3.org/1999/xhtml">
            <head>
                <title>Svarrapport</title>
                <meta http-equiv="content-type" content="text/html; charset=utf-8"/>
                <style type="text/css">
                    body
                    {
                        background-color: #ffffff;
                        color: #000000;
                    }

                    @media screen {

                        .Top {
                            width:95%;
                            margin: 1px;
                            border:1px solid gray;
                            padding:1em;
                            background-color: #eeeeee;
                            font-family: arial;
                            font-size: 11px;
                        }

                        .Header {
                            width: 100%;
                            border:1px solid black;
                            overflow: visible;
                            padding: 1px;
                        }

                        .HeaderContent{
                            width: 100%;
                            padding: 1px;
                        }

                        .Caption {
                            width: 100px;
                            padding:1px;
                            background-color: #CAE8EA;
                            margin-right: 2px;
                            float: left;
                            clear: both;
                            font-weight: bold;
                        }

                        .Content {
                            padding:1px;
                            background-color:  white;
                            margin-right: 1px;
                        }

                        .Content div {
                            display:inline;
                        }

                        .NoScreen {
                            display: none;
                        }

                    }

                    @media print {

                        .Top {
                            margin: 1px;
                            font-family: arial;
                            font-size: 11px;
                            overflow: visible;
                        }

                        .HeaderContent {
                            width: 47.5%;
                            height: 68px;
                            min-height: 68px;
                            max-height: 68px;
                            border: solid black 1px;
                            padding: 0.5em;
                            float: left;
                            overflow: hidden;
                        }

                        .Caption {
                            float: left;
                            display:inline;
                            font-weight: bold;
                        }

                        .NoPrint {
                            display: none;
                        }

                    }

                    .Document {
                        width:95%;
                        clear:left;
                        margin: 1px;
                        border:1px solid gray;
                        padding:1em;
                        background-color: #eeeeee;
                        font-family: arial;
                        font-size: 11px;
                    }

                    .Document h1 {
                        padding:0.3em;
                        color:white;
                        background-color:gray;
                        border:1px solid gray;
                        font-size: 1.3em;
                        line-height:0.7em;
                        margin-bottom:0.3em;
                    }

                    .Document h2 {
                        padding:0.4em;
                        color:white;
                        background-color: gray;
                        border:1px solid gray;
                        font-size: 1.0em;
                        line-height:0.7em;
                        margin-bottom:0.3em;
                    }

                    .Document h3 {
                        padding:0.2em;
                        color:black;
                        font-size: 1em;
                        line-height:0.1em;
                        margin-bottom:0.3em
                    }

                    .Document table {
                        border:1px solid black;
                        width:100%;
                    }

                    .Document th {
                        background-color: #CAE8EA;
                        border-style:none;
                        font-family:Arial;
                        font-size:11px;
                        text-align: left;
                        text-valign: top;
                    }

                    .Document td {
                        background-color: white;
                        border-style: none;
                        text-align: left;
                        font-size: 11px;
                    }

                    th.h2 {
                        color:white;
                        background-color: gray;
                        border:1px solid gray;
                        font-size: 12px;
                        line-height:0.9em;
                        width: 12%;
                    }

                    th.h3 {
                        width: 12%;
                    }

                    td.Emphasized {
                        background-color:#CAE8EA;
                        width: 12%;
                    }

                    .Box {
                        border:1px solid black;
                        padding:1px;
                        overflow: hidden;
                    }

                    .MainCaption {
                        padding:1px;
                        background-color: #CAE8EA;
                        margin: 1px;
                        display: inline;
                        font-weight: bold;
                        white-space: nowrap;
                    }

                    .MainContent {
                        padding:1px;
                        padding-right:1cm;
                        background-color:  white;
                        margin: 1px;
                        display: inline;
                        white-space: nowrap;
                    }

                    .Block {
                        float: left;
                    }

                    .MsgInfo {
                        width: 95%;
                        margin: 1px;
                        border:1px solid gray;
                        padding:0.5em;
                        background-color: #eeeeee;
                        font-family: arial;
                        font-size: 0.8em;
                    }

                    .Organisation .HealthcareProfessional {
                        margin-left: 2em;
                    }


                    #InformasjonOmForsendelsen {
                        font-size: 0.7em;
                        border: 1px solid;
                        padding:0.5em;
                        margin: 0.5em;
                    }

                    #FellesMeny ul {
                        margin: 0;
                        padding: 0;
                        list-style-type: none;
                        list-style-image: none;
                    }

                    #FellesMeny li {
                        display:inline;
                    }

                    #FellesMeny a {
                        text-decoration:none;
                        color: blue;
                        white-space:nowrap;
                        padding-right:1em;
                        width:15em;
                    }

                    #FellesMeny a:hover {
                        color: purple;
                    }

                </style>
            </head>
            <body>
                <xsl:apply-templates/>
            </body>
        </html>
    </xsl:template>
    <xsl:template name="FellesMeny">
        <xsl:param name="position"/>
        <div id="FellesMeny" class="FellesMeny">
            <ul>
                <li>
                    <xsl:variable name="temp1" select="concat('ServReport',$position)"/>
                    <a href="#{$temp1}">Meldingsinformasjon</a>
                </li>
                <xsl:if test=".//lsr:Patient/lsr:AnalysedSubject or .//lsr:Animal/lsr:AnalysedSubject or .//lsr:Material/lsr:AnalysedSubject">
                    <li>
                        <xsl:variable name="temp2" select="concat('AnalysedSubject',$position)"/>
                        <a href="#{$temp2}">Analysert materiale</a>
                    </li>
                </xsl:if>
                <xsl:if test=".//lsr:ResultItem">
                    <li>
                        <xsl:variable name="temp3" select="concat('ResultItem',$position)"/>
                        <a href="#{$temp3}">Undersøkelsesresultat</a>
                    </li>
                </xsl:if>
                <xsl:if test=".//lsr:ServReq">
                    <li>
                        <xsl:variable name="temp4" select="concat('ServReq',$position)"/>
                        <a href="#{$temp4}">Opprinnelig rekvisisjon</a>
                    </li>
                </xsl:if>
                <xsl:if test=".//lsr:Patient/lsr:BasisForHealthServices or .//lsr:Patient/lsr:Sex or .//lsr:Patient/lsr:DateOfBirth or .//lsr:Patient/lsr:DateOfDeath or .//lsr:Patient/lsr:AdmLocation or .//lsr:Patient/lsr:AdditionalId or .//lsr:Patient/lsr:InfItem or .//lsr:Patient/lsr:Patient">
                    <li>
                        <xsl:variable name="temp5" select="concat('Patient',$position)"/>
                        <a href="#{$temp5}">Øvrig pasientinformasjon</a>
                    </li>
                </xsl:if>
                <xsl:if test=".//lsr:Animal/lsr:Sex or .//lsr:Animal/lsr:Animal">
                    <li>
                        <xsl:variable name="temp6" select="concat('Animal',$position)"/>
                        <a href="#{$temp6}">Øvrig undersøkelsesdyr-informasjon</a>
                    </li>
                </xsl:if>
                <xsl:if test=".//lsr:ServProvider | .//lsr:Requester | .//lsr:PaymentResponsible | .//lsr:CopyDest | lsr:RelServProv | .//lsr:RelServProv[descendant::lsr:Id] | .//lsr:ResponsibleHcp | .//lsr:Patient/lsr:AdmLocation">
                    <li>
                        <xsl:variable name="temp7" select="concat('HCP',$position)"/>
                        <a href="#{$temp7}">Øvrig informasjon helsetjenesteenheter</a>
                    </li>
                </xsl:if>
                <xsl:if test=".//lsr:RefDoc">
                    <li>
                        <xsl:variable name="temp8" select="concat('RefDoc',$position)"/>
                        <a href="#{$temp8}">Referert dokument</a>
                    </li>
                </xsl:if>
            </ul>
        </div>
    </xsl:template>
    <xsl:template match="lsr:Message">
        <!-- utelater meldingsid og kommunikasjonsinformasjon -->
        <xsl:call-template name="Header"/>
        <xsl:call-template name="ResultBody"/>
        <xsl:call-template name="Footer"/>
    </xsl:template>
    <!-- Meldingsstart slutt -->
    <!-- Header - avsender og mottaker-informasjon -->
    <xsl:template name="Header">
        <div class="Top">
            <div class="Header">
                <div class="HeaderContent">
                    <div class="Caption">Avsender &#160;</div>
                    <div class="Content">
                        <xsl:for-each select="lsr:ServReport/lsr:ServProvider/lsr:HCP">
                            <xsl:call-template name="Header-HCP"/>
                            <xsl:call-template name="Header-TeleAddress"/>
                        </xsl:for-each>
                        <xsl:for-each select="lsr:ServReport/lsr:RelServProv/lsr:HCP">
                            <div><b>
                                <xsl:choose>
                                    <xsl:when test="../lsr:Relation/@DN">
                                        <xsl:value-of select="../lsr:Relation/@DN"/>&#160;</xsl:when>
                                    <xsl:otherwise>Tilknyttet tjenesteyter&#160;</xsl:otherwise>
                                </xsl:choose>
                            </b></div>
                            <xsl:call-template name="Header-HCP"/>
                            <xsl:call-template name="Header-TeleAddress"/>
                        </xsl:for-each>
                    </div>
                </div>
                <xsl:choose>
                    <xsl:when test="lsr:ServReport/lsr:Patient">
                        <div class="HeaderContent">
                            <xsl:choose>
                                <xsl:when test="lsr:ServReport/lsr:Patient">
                                    <div class="Caption">Pasient &#160;</div>
                                    <div class="Content">
                                        <xsl:for-each select="lsr:ServReport/lsr:Patient">
                                            <div><xsl:value-of select="lsr:Name"/>&#160;<b><xsl:value-of select="lsr:TypeOffId/@V"/>:&#160;</b><xsl:value-of select="substring(lsr:OffId, 1,6)"/>&#160;<xsl:value-of select="substring(lsr:OffId, 7)"/>&#160;</div>
                                            <xsl:call-template name="Header-Address"/>
                                            <xsl:call-template name="Header-TeleAddress"/>
                                        </xsl:for-each>
                                    </div>
                                </xsl:when>
                                <xsl:when test="lsr:ServReport/lsr:Animal">
                                    <div class="Caption">Undersøkelsesdyr<xsl:if test="lsr:Species">:&#160;<xsl:value-of select="lsr:Species"/></xsl:if></div>
                                    <div class="Content">
                                        <xsl:for-each select="lsr:ServReport/lsr:Animal">
                                            <div><b>Navn:&#160;</b>
                                                <xsl:choose>
                                                    <xsl:when test="lsr:Name">
                                                        <xsl:value-of select="lsr:Name"/>&#160;</xsl:when>
                                                    <xsl:otherwise>Ukjent&#160;</xsl:otherwise>
                                                </xsl:choose></div>
                                            <div><b>Eier:&#160;</b>
                                                <xsl:choose>
                                                    <xsl:when test="lsr:NameOwner">
                                                        <xsl:value-of select="lsr:NameOwner"/>&#160;</xsl:when>
                                                    <xsl:otherwise>Ukjent&#160;</xsl:otherwise>
                                                </xsl:choose></div>
                                        </xsl:for-each>
                                    </div>
                                </xsl:when>
                                <xsl:otherwise>
                                    <div class="Caption">Materiale &#160;</div>
                                    <div class="Content">
                                        <xsl:for-each select="lsr:ServReport/lsr:Material">
                                            <div><b>Beskrivelse:&#160;</b>
                                                <xsl:choose>
                                                    <xsl:when test="lsr:InvMaterial"><xsl:value-of select="lsr:InvMaterial"/>&#160;</xsl:when>
                                                    <xsl:when test="lsr:Material/lsr:InvMaterial">,&#160;<xsl:value-of select="lsr:Material/lsr:InvMaterial"/>&#160;</xsl:when>
                                                    <xsl:otherwise>Ukjent&#160;</xsl:otherwise>
                                                </xsl:choose></div>
                                        </xsl:for-each>
                                    </div>
                                </xsl:otherwise>
                            </xsl:choose>
                        </div>
                    </xsl:when>
                    <xsl:otherwise>
                        <div class="HeaderContent">
                            <div class="NoScreen">&#160;</div>
                        </div>
                    </xsl:otherwise>
                </xsl:choose>
                <div class="HeaderContent">
                    <div class="Caption">Mottaker &#160;</div>
                    <div class="Content">
                        <xsl:for-each select="lsr:ServReport/lsr:Requester/lsr:HCP">
                            <xsl:call-template name="Header-HCP"/>
                            <xsl:call-template name="Header-Address"/>
                        </xsl:for-each>
                    </div>
                </div>
                <xsl:choose>
                    <xsl:when test="lsr:ServReport/lsr:CopyDest/lsr:HCP">
                        <div class="HeaderContent">
                            <div class="Caption">Kopimottaker(e) &#160;</div>
                            <div class="Content">
                                <xsl:for-each select="lsr:ServReport/lsr:CopyDest/lsr:HCP">
                                    <xsl:call-template name="Header-HCP"/>
                                    <xsl:call-template name="Header-Address"/>
                                </xsl:for-each>
                            </div>
                        </div>
                    </xsl:when>
                    <xsl:otherwise>
                        <div class="HeaderContent">
                            <div class="NoScreen">&#160;</div>
                        </div>
                    </xsl:otherwise>
                </xsl:choose>
            </div>
        </div>
    </xsl:template>
    <!-- Headervisning av Helsetjenesteenhet -->
    <xsl:template name="Header-HCP">
        <xsl:for-each select="lsr:HCProf">
            <div><xsl:if test="lsr:Type/@DN"><xsl:value-of select="lsr:Type/@DN"/>:&#160;</xsl:if>
                <xsl:if test="lsr:Name"><xsl:value-of select="lsr:Name"/>&#160;</xsl:if></div>
        </xsl:for-each>
        <xsl:for-each select="lsr:Inst">
            <div><xsl:value-of select="lsr:Name"/>&#160;</div>
            <xsl:for-each select="lsr:Dept"><div><div  class="NoPrint">&#160;</div><xsl:value-of select="lsr:Name"/>&#160;</div></xsl:for-each>
            <div><xsl:value-of select="lsr:HCPerson/lsr:Name"/>&#160;</div>
        </xsl:for-each>
    </xsl:template>
    <!-- Headervisning av Adresse -->
    <xsl:template name="Header-Address">
        <xsl:if test="lsr:Type and (lsr:PostalCode or lsr:City)">
            <xsl:if test="lsr:Type">
                <div class="NoPrint">&#160;<b><xsl:choose><xsl:when test="lsr:Type/@DN"><xsl:value-of select="lsr:Type/@DN"/>:&#160;</xsl:when><xsl:otherwise>Adresse:&#160;</xsl:otherwise></xsl:choose></b></div>
            </xsl:if>
            <xsl:if test="lsr:StreetAdr">
                <div><xsl:value-of select="lsr:StreetAdr"/></div>
            </xsl:if>
            <xsl:if test="lsr:PostalCode or lsr:City">
                <xsl:if test="lsr:StreetAdr">
                    <div class="NoPrint">,</div>
                </xsl:if>
                <div><xsl:value-of select="lsr:PostalCode"/>&#160;<xsl:value-of select="lsr:City"/></div>
            </xsl:if>
            <xsl:if test="lsr:CityDistr">
                <div class="NoPrint">,</div>
                <div><xsl:value-of select="lsr:CityDistr/@DN"/></div>
            </xsl:if>
            <xsl:if test="lsr:County">
                <div class="NoPrint">,</div>
                <div><xsl:value-of select="lsr:County/@DN"/></div>
            </xsl:if>
            <xsl:if test="lsr:Country">
                <div class="NoPrint">,</div>
                <div><xsl:value-of select="lsr:Country/@DN"/></div>
            </xsl:if>
            <xsl:for-each select="lsr:TeleAddress">
                <div class="NoPrint">&#160;</div>
                <xsl:apply-templates select="."/>
            </xsl:for-each>
        </xsl:if>
    </xsl:template>
    <!-- Headervisning av Telekommunikasjon -->
    <xsl:template name="Header-TeleAddress">
        <xsl:if test="starts-with(@V, &quot;tel:&quot;) or starts-with(@V, &quot;callto:&quot;)">
            <div><b>Telefon:</b>&#160;<xsl:value-of select="substring-after(@V, &quot;:&quot;)"/>&#160;</div>
        </xsl:if>
        <xsl:if test="starts-with(@V, &quot;fax:&quot;)">
            <div><b>Faks:</b>&#160;<xsl:value-of select="substring-after(@V, &quot;:&quot;)"/>&#160;</div>
        </xsl:if>
        <xsl:if test="starts-with(@V, &quot;mailto:&quot;)">
            <div><b>e-post:</b>&#160;<xsl:value-of select="substring-after(@V, &quot;:&quot;)"/>&#160;</div>
        </xsl:if>
    </xsl:template>
    <!-- Header slutt -->
    <!-- Meldingshodet - Dokumentopplysninger -->
    <xsl:template name="Footer">
        <div class="Document">
            <h2>Dokumentinformasjon</h2>
            <div class="Box">
                <div class="Block">
                    <div class="MainCaption">Melding sendt</div>
                    <div class="MainContent"><xsl:call-template name="skrivUtDateTime"><xsl:with-param name="oppgittTid" select="lsr:GenDate/@V"/></xsl:call-template></div>
                </div>
                <div class="Block">
                    <div class="MainCaption">Meldingsid</div>
                    <div class="MainContent"><xsl:value-of select="lsr:MsgId"/></div>
                </div>
            </div>
        </div>
    </xsl:template>
    <!-- Hoveddokument -->
    <xsl:template name="ResultBody">
        <div class="Document">
            <xsl:for-each select="lsr:ServReport">
                <xsl:variable name="position" select="position()"/>
                <!-- Tabell for svarrapport -->
                <h1>Svarrapport
                    <xsl:choose>
                        <xsl:when test="lsr:MsgDescr/@DN">-&#160;<xsl:value-of select="lsr:MsgDescr/@DN"/>
                        </xsl:when>
                        <xsl:otherwise>-&#160;<xsl:value-of select="lsr:MsgDescr/@V"/>
                        </xsl:otherwise>
                    </xsl:choose>
                </h1>
                <xsl:call-template name="FellesMeny"><xsl:with-param name="position" select="position()"/></xsl:call-template><br/>
                <xsl:variable name="id1"><xsl:value-of select="concat('ServReport',$position)"/></xsl:variable>
                <h3 id="{$id1}">Meldingsinformasjon</h3>
                <table>
                    <tbody>
                        <xsl:apply-templates select=".">
                            <xsl:with-param name="col" select="$servreport-col"/>
                        </xsl:apply-templates>
                    </tbody>
                </table>
                <!-- Tabell for analysert objekt -->
                <xsl:if test=".//lsr:AnalysedSubject">
                    <xsl:variable name="id2"><xsl:value-of select="concat('AnalysedSubject',$position)"/></xsl:variable>
                    <h3 id="{$id2}">Analysert materiale</h3>
                    <table>
                        <tbody>
                            <xsl:for-each select="lsr:Patient/lsr:AnalysedSubject | lsr:Animal/lsr:AnalysedSubject | lsr:Material/lsr:AnalysedSubject">
                                <xsl:apply-templates select=".">
                                    <xsl:with-param name="col" select="$analysed-col"/>
                                </xsl:apply-templates>
                            </xsl:for-each>
                        </tbody>
                    </table>
                </xsl:if>
                <xsl:if test=".//lsr:ResultItem">
                    <xsl:variable name="id3"><xsl:value-of select="concat('ResultItem',$position)"/></xsl:variable>
                    <!-- Tabell for Undersøkelsesresultat -->
                    <h3 id="{$id3}">Undersøkelsesresultat</h3>
                    <table>
                        <tbody>
                            <tr>
                                <xsl:if test=".//lsr:ResultItem/lsr:Investigation">
                                    <th colspan="{$investigation-col}">Undersøkelse</th>
                                </xsl:if>
                                <xsl:if test=".//lsr:ResultItem/lsr:TextResult">
                                    <th>
                                        <xsl:if test=".//lsr:ResultItem/lsr:IdResultItem">(Id)</xsl:if>
                                        Type tekstsvar
                                    </th>
                                    <th colspan="{$textresult-col}">
                                        <xsl:if test=".//lsr:ResultItem/lsr:RefIdResultItem">(RefId)</xsl:if>
                                        Tekstsvar
                                    </th>
                                </xsl:if>
                                <xsl:if test=".//lsr:ResultItem/lsr:Interval">
                                    <th>
                                        <xsl:if test=".//lsr:ResultItem/lsr:IdResultItem">(Id)</xsl:if>
                                        Benevning
                                    </th>
                                    <th colspan="{$interval-col}">
                                        <xsl:if test=".//lsr:ResultItem/lsr:RefIdResultItem">(RefId)</xsl:if>
                                        Intervalsvar
                                    </th>
                                </xsl:if>
                                <xsl:if test=".//lsr:ResultItem/lsr:DateResult">
                                    <th colspan="{$dateresult-col}">
                                        <xsl:if test=".//lsr:ResultItem/lsr:RefIdResultItem">(RefId)</xsl:if>
                                        Datosvar
                                    </th>
                                </xsl:if>
                                <xsl:if test=".//lsr:ResultItem/lsr:NumResult">
                                    <th>
                                        <xsl:if test=".//lsr:ResultItem/lsr:IdResultItem">(Id)</xsl:if>
                                        Benevning
                                    </th>
                                    <th colspan="{$numresult-col}">
                                        <xsl:if test=".//lsr:ResultItem/lsr:RefIdResultItem">(RefId)</xsl:if>
                                        Svar
                                    </th>
                                </xsl:if>
                                <xsl:if test=".//lsr:ResultItem/lsr:RefInterval">
                                    <th colspan="{$refinterval-col}">Referanseinterval</th>
                                </xsl:if>
                                <xsl:if test=".//lsr:ResultItem/lsr:DevResultInd">
                                    <th colspan="{$devresultind-col}">Avvik</th>
                                </xsl:if>
                                <xsl:if test=".//lsr:ResultItem/lsr:InvDate">
                                    <th colspan="{$invdate-col}">Tidspunkt for undersøkelsen</th>
                                </xsl:if>
                                <xsl:if test=".//lsr:ResultItem/lsr:DescrDate">
                                    <th colspan="{$descrdate-col}">Tidspunkt for beskrivelsen</th>
                                </xsl:if>
                                <xsl:if test=".//lsr:ResultItem/lsr:CounterSignDate">
                                    <th colspan="{$countersign-col}">Tidspunkt for kontrasignering</th>
                                </xsl:if>
                                <xsl:if test=".//lsr:ResultItem/lsr:MedicalValidationDate">
                                    <th colspan="{$medvaldate-col}">Tidspunkt for medisinsk validering</th>
                                </xsl:if>
                                <xsl:if test=".//lsr:ResultItem/lsr:Accredited">
                                    <th colspan="{$accred-col}">Akkreditert</th>
                                </xsl:if>
                                <xsl:if test=".//lsr:ResultItem/lsr:StructuredInfo">
                                    <th colspan="{$structured-col}" width="{$structured-col-width}">Strukturert informasjon</th>
                                </xsl:if>
                                <xsl:if test=".//lsr:ResultItem/lsr:RelServProv">
                                    <th colspan="{$relserv-col}">Tilknyttet tjenesteyter</th>
                                </xsl:if>
                                <xsl:if test=".//lsr:ResultItem/lsr:Investigation/lsr:Comment | .//lsr:ResultItem/lsr:Investigation/lsr:Spec | .//lsr:ResultItem/lsr:Comment | .//lsr:ResultItem/lsr:DiagComment">
                                    <th colspan="{$merknad-col}">Merknad</th>
                                </xsl:if>
                            </tr>
                            <xsl:for-each select=".//lsr:ResultItem">
                                <xsl:apply-templates select=".">
                                    <xsl:with-param name="col" select="$result-col"/>
                                </xsl:apply-templates>
                            </xsl:for-each>
                            <xsl:if test=".//lsr:ResultItem/lsr:Investigation/lsr:Comment | .//lsr:ResultItem/lsr:Investigation/lsr:Spec | .//lsr:ResultItem/lsr:Comment | .//lsr:ResultItem/lsr:DiagComment">
                                <tr>
                                    <th colspan="{$result-col}">
                                        <hr/>
                                    </th>
                                </tr>
                            </xsl:if>
                            <xsl:if test=".//lsr:ResultItem/lsr:Investigation/lsr:Comment">
                                <tr>
                                    <th rowspan="{count(.//lsr:ResultItem/lsr:Investigation/lsr:Comment)}">Kommentarer til undersøkelsene</th>
                                    <xsl:for-each select=".//lsr:ResultItem/lsr:Investigation/lsr:Comment">
                                        <xsl:choose>
                                            <xsl:when test="position()=1">
                                                <td colspan="{($result-col)-1}">
                                                    <b>
                                                        <xsl:value-of select="$und-komm"/>
                                                        <xsl:value-of select="position()"/>)&#160;</b>
                                                    <xsl:call-template name="line-breaks">
                                                        <xsl:with-param name="text" select="."/>
                                                    </xsl:call-template>
                                                </td>
                                            </xsl:when>
                                            <xsl:otherwise>
                                                <tr>
                                                    <td colspan="{($result-col)-1}">
                                                        <b>
                                                            <xsl:value-of select="$und-komm"/>
                                                            <xsl:value-of select="position()"/>)&#160;</b>
                                                        <xsl:call-template name="line-breaks">
                                                            <xsl:with-param name="text" select="."/>
                                                        </xsl:call-template>
                                                    </td>
                                                </tr>
                                            </xsl:otherwise>
                                        </xsl:choose>
                                    </xsl:for-each>
                                </tr>
                            </xsl:if>
                            <xsl:if test=".//lsr:ResultItem/lsr:Investigation/lsr:Spec">
                                <tr>
                                    <th rowspan="{count(.//lsr:ResultItem/lsr:Investigation/lsr:Spec)}">Spesifisering av undersøkelsene</th>
                                    <xsl:for-each select=".//lsr:ResultItem/lsr:Investigation/lsr:Spec">
                                        <xsl:choose>
                                            <xsl:when test="position()=1">
                                                <td colspan="{($result-col)-1}">
                                                    <b>
                                                        <xsl:value-of select="$und-spes"/>
                                                        <xsl:value-of select="position()"/>)&#160;</b>
                                                    <xsl:choose>
                                                        <xsl:when test="@OT">
                                                            <xsl:value-of select="@OT"/>&#160;</xsl:when>
                                                        <xsl:otherwise>
                                                            <xsl:value-of select="@DN"/>&#160;</xsl:otherwise>
                                                    </xsl:choose>
                                                </td>
                                            </xsl:when>
                                            <xsl:otherwise>
                                                <tr>
                                                    <td colspan="{($result-col)-1}">
                                                        <b>
                                                            <xsl:value-of select="$und-spes"/>
                                                            <xsl:value-of select="position()"/>)&#160;</b>
                                                        <xsl:choose>
                                                            <xsl:when test="@OT">
                                                                <xsl:value-of select="@OT"/>&#160;</xsl:when>
                                                            <xsl:otherwise>
                                                                <xsl:value-of select="@DN"/>&#160;</xsl:otherwise>
                                                        </xsl:choose>
                                                    </td>
                                                </tr>
                                            </xsl:otherwise>
                                        </xsl:choose>
                                    </xsl:for-each>
                                </tr>
                            </xsl:if>
                            <xsl:if test=".//lsr:ResultItem/lsr:Comment">
                                <tr>
                                    <th rowspan="{count(.//lsr:ResultItem/lsr:Comment)}">Kommentarer til resultatene</th>
                                    <xsl:for-each select=".//lsr:ResultItem/lsr:Comment">
                                        <xsl:choose>
                                            <xsl:when test="position()=1">
                                                <td colspan="{($result-col)-1}">
                                                    <b>
                                                        <xsl:value-of select="$res-komm"/>
                                                        <xsl:value-of select="position()"/>)&#160;</b>
                                                    <xsl:value-of select="."/>
                                                </td>
                                            </xsl:when>
                                            <xsl:otherwise>
                                                <tr>
                                                    <td colspan="{($result-col)-1}">
                                                        <b>
                                                            <xsl:value-of select="$res-komm"/>
                                                            <xsl:value-of select="position()"/>)&#160;</b>
                                                        <xsl:value-of select="."/>
                                                    </td>
                                                </tr>
                                            </xsl:otherwise>
                                        </xsl:choose>
                                    </xsl:for-each>
                                </tr>
                            </xsl:if>
                            <xsl:if test=".//lsr:ResultItem/lsr:DiagComment">
                                <tr>
                                    <th rowspan="{count(.//lsr:ResultItem/lsr:DiagComment)}">Kommentarer som diagnose</th>
                                    <xsl:for-each select=".//lsr:ResultItem/lsr:DiagComment">
                                        <xsl:choose>
                                            <xsl:when test="position()=1">
                                                <td colspan="{($result-col)-1}">
                                                    <b>
                                                        <xsl:value-of select="$dia-komm"/>
                                                        <xsl:value-of select="position()"/>)&#160;</b>
                                                    <xsl:choose>
                                                        <xsl:when test="lsr:Concept/@OT">
                                                            <xsl:value-of select="lsr:Concept/@OT"/>&#160;</xsl:when>
                                                        <xsl:otherwise>
                                                            <xsl:value-of select="lsr:Concept/@DN"/>&#160;</xsl:otherwise>
                                                    </xsl:choose>
                                                    <xsl:for-each select="lsr:Modifier">
                                                        <b>Moderator-navn:&#160;</b>
                                                        <xsl:choose>
                                                            <xsl:when test="lsr:Name/@DN">
                                                                <xsl:value-of select="lsr:Name/@DN"/>&#160;</xsl:when>
                                                            <xsl:otherwise>
                                                                <b>Kodet:&#160;</b>
                                                                <xsl:value-of select="lsr:Name/@V"/>&#160;</xsl:otherwise>
                                                        </xsl:choose>
                                                        <xsl:for-each select="lsr:Value">
                                                            <b>Verdi:&#160;</b>
                                                            <xsl:choose>
                                                                <xsl:when test="@OT">
                                                                    <xsl:value-of select="@OT"/>&#160;</xsl:when>
                                                                <xsl:otherwise>
                                                                    <xsl:value-of select="@DN"/>&#160;</xsl:otherwise>
                                                            </xsl:choose>
                                                        </xsl:for-each>
                                                    </xsl:for-each>
                                                </td>
                                            </xsl:when>
                                            <xsl:otherwise>
                                                <tr>
                                                    <td colspan="{($result-col)-1}">
                                                        <b>
                                                            <xsl:value-of select="$dia-komm"/>
                                                            <xsl:value-of select="position()"/>)&#160;</b>
                                                        <xsl:choose>
                                                            <xsl:when test="lsr:Concept/@OT">
                                                                <xsl:value-of select="lsr:Concept/@OT"/>&#160;</xsl:when>
                                                            <xsl:otherwise>
                                                                <xsl:value-of select="lsr:Concept/@DN"/>&#160;</xsl:otherwise>
                                                        </xsl:choose>
                                                        <xsl:for-each select="lsr:Modifier">
                                                            <b>Moderator-navn:&#160;</b>
                                                            <xsl:choose>
                                                                <xsl:when test="lsr:Name/@DN">
                                                                    <xsl:value-of select="lsr:Name/@DN"/>&#160;</xsl:when>
                                                                <xsl:otherwise>
                                                                    <b>Kodet:&#160;</b>
                                                                    <xsl:value-of select="lsr:Name/@V"/>&#160;</xsl:otherwise>
                                                            </xsl:choose>
                                                            <xsl:for-each select="lsr:Value">
                                                                <b>Verdi:&#160;</b>
                                                                <xsl:choose>
                                                                    <xsl:when test="@OT">
                                                                        <xsl:value-of select="@OT"/>&#160;</xsl:when>
                                                                    <xsl:otherwise>
                                                                        <xsl:value-of select="@DN"/>&#160;</xsl:otherwise>
                                                                </xsl:choose>
                                                            </xsl:for-each>
                                                        </xsl:for-each>
                                                    </td>
                                                </tr>
                                            </xsl:otherwise>
                                        </xsl:choose>
                                    </xsl:for-each>
                                </tr>
                            </xsl:if>
                        </tbody>
                    </table>
                </xsl:if>
                <!-- Tabell for oprinnelig rekvisisjon -->
                <xsl:for-each select="lsr:ServReq">
                    <xsl:variable name="id4"><xsl:value-of select="concat('ServReq',$position)"/></xsl:variable>
                    <h3 id="{$id4}">Opprinnelig rekvisisjon
                        <xsl:if test="lsr:MsgDescr">
                            <xsl:choose>
                                <xsl:when test="lsr:MsgDescr/@DN">-&#160;<xsl:value-of select="lsr:MsgDescr/@DN"/>
                                </xsl:when>
                                <xsl:otherwise>-&#160;<xsl:value-of select="lsr:MsgDescr/@V"/>
                                </xsl:otherwise>
                            </xsl:choose>
                        </xsl:if>
                        <xsl:if test="lsr:ServType">
                            <xsl:choose>
                                <xsl:when test="lsr:ServType/@DN">,&#160;<xsl:value-of select="lsr:ServType/@DN"/>
                                </xsl:when>
                                <xsl:otherwise>,&#160;<xsl:value-of select="lsr:ServType/@V"/>
                                </xsl:otherwise>
                            </xsl:choose>
                        </xsl:if>
                    </h3>
                    <table>
                        <tbody>
                            <xsl:apply-templates select=".">
                                <xsl:with-param name="col" select="$servreq-col"/>
                            </xsl:apply-templates>
                        </tbody>
                    </table>
                </xsl:for-each>
                <!-- Tabell for øvrig pasientinformasjon -->
                <xsl:for-each select="lsr:Patient">
                    <xsl:if test="lsr:BasisForHealthServices or lsr:Sex or lsr:DateOfBirth or lsr:DateOfDeath or lsr:AdmLocation or lsr:AdditionalId or lsr:InfItem or lsr:Patient">
                        <xsl:variable name="id5"><xsl:value-of select="concat('Patient',$position)"/></xsl:variable>
                        <h3 id="{$id5}">Øvrig pasientinformasjon&#160;</h3>
                        <table>
                            <tbody>
                                <xsl:apply-templates select=".">
                                    <xsl:with-param name="col" select="$pasient-col"/>
                                </xsl:apply-templates>
                            </tbody>
                        </table>
                    </xsl:if>
                </xsl:for-each>
                <!-- Tabell for øvrig informasjon om undersøkelsesobjekt: dyr -->
                <xsl:for-each select="lsr:Animal">
                    <xsl:if test="lsr:Sex or lsr:Animal">
                        <xsl:variable name="id6"><xsl:value-of select="concat('Animal',$position)"/></xsl:variable>
                        <h3 id="{$id6}">Øvrig undersøkelsesdyr-informasjon</h3>
                        <table>
                            <tbody>
                                <xsl:apply-templates select=".">
                                    <xsl:with-param name="col" select="$pasient-col"/>
                                </xsl:apply-templates>
                            </tbody>
                        </table>
                    </xsl:if>
                </xsl:for-each>
                <!-- Tabell for alle tilknyttede helsetjenesteenheter -->
                <xsl:variable name="id7"><xsl:value-of select="concat('HCP',$position)"/></xsl:variable>
                <h3 id="{$id7}">Øvrig informasjon helsetjenesteenheter</h3>
                <table>
                    <tbody>
                        <xsl:for-each select="lsr:ServProvider | lsr:Requester | lsr:PaymentResponsible | lsr:CopyDest | lsr:RelServProv | .//lsr:RelServProv[descendant::lsr:Id] | .//lsr:ResponsibleHcp | lsr:Patient/lsr:AdmLocation">
                            <xsl:apply-templates select=".">
                                <xsl:with-param name="col" select="$hcp-col"/>
                            </xsl:apply-templates>
                        </xsl:for-each>
                    </tbody>
                </table>
                <!-- Tabell for referert dokument -->
                <xsl:if test="lsr:RefDoc">
                    <xsl:variable name="id8"><xsl:value-of select="concat('RefDoc',$position)"/></xsl:variable>
                    <h3 id="{$id8}">Referert dokument</h3>
                    <table>
                        <tbody>
                            <xsl:for-each select="lsr:RefDoc">
                                <xsl:apply-templates select=".">
                                    <xsl:with-param name="col" select="$refdoc-col"/>
                                </xsl:apply-templates>
                            </xsl:for-each>
                        </tbody>
                    </table>
                </xsl:if>
            </xsl:for-each>
        </div>
    </xsl:template>
    <!-- Visning av Svarrapport -->
    <xsl:template match="lsr:ServReport">
        <xsl:param name="col"/>
        <tr>
            <th class="h3">Status</th>
            <td>
                <xsl:value-of select="lsr:ServType/@DN"/> - <xsl:value-of select="lsr:Status/@DN"/>
            </td>
            <th>Utstedt-dato</th>
            <td colspan="{(($col)-2-count(lsr:ServType)*2)*number(not(lsr:ApprDate | lsr:CancellationCode))+1}">
                <xsl:call-template name="skrivUtDateTime"><xsl:with-param name="oppgittTid" select="lsr:IssueDate/@V"></xsl:with-param></xsl:call-template>
            </td>
            <xsl:if test="lsr:ApprDate">
                <th>Godkjent-dato</th>
                <td colspan="{(($col)-2-count(lsr:ServType | lsr:IssueDate)*2)*number(not(lsr:CancellationCode))+1}">
                    <xsl:call-template name="skrivUtDateTime"><xsl:with-param name="oppgittTid" select="lsr:ApprDate/@V"></xsl:with-param></xsl:call-template>
                </td>
            </xsl:if>
            <xsl:if test="lsr:CancellationCode">
                <th>Årsak til kansellering</th>
                <td colspan="{($col)-1-count(lsr:ServType | lsr:IssueDate | lsr:ApprDate)*2}">
                    <xsl:value-of select="lsr:CancellationCode/@DN"/>
                </td>
            </xsl:if>
        </tr>
        <xsl:if test="lsr:Comment or lsr:CodedComment">
            <tr>
                <th>Kommentar til svarrapporten</th>
                <td colspan="{($col)-1}">
                    <xsl:call-template name="line-breaks">
                        <xsl:with-param name="text" select="lsr:Comment"/>
                    </xsl:call-template>&#160;
                    <xsl:for-each select="lsr:CodedComment">
                        <b>Kodet:&#160;</b>
                        <xsl:value-of select="@DN"/>&#160;
                    </xsl:for-each>
                </td>
            </tr>
        </xsl:if>
    </xsl:template>
    <!-- Visning av Opprinnelig rekvisisjon -->
    <xsl:template match="lsr:ServReq">
        <xsl:param name="col"/>
        <xsl:if test="lsr:IssueDate or lsr:ReceiptDate or lsr:RequestedPrioReport or lsr:PaymentCat or lsr:Reservation">
            <tr>
                <xsl:if test="lsr:IssueDate">
                    <th>Utstedt-dato</th>
                    <td colspan="{(($col)-2)*number(not(lsr:ReceiptDate | lsr:RequestedPrioReport | lsr:PaymentCat | lsr:Reservation))+1}">
                        <xsl:value-of select="substring(lsr:IssueDate/@V,1,10)"/>
                        <xsl:if test="contains(lsr:IssueDate/@V, 'T')">&#160;kl.<xsl:value-of select="substring(lsr:IssueDate/@V,12,5)"/>
                        </xsl:if>
                    </td>
                </xsl:if>
                <xsl:if test="lsr:ReceiptDate">
                    <th>Mottatt-dato</th>
                    <td colspan="{(($col)-2-count(lsr:IssueDate)*2)*number(not(lsr:RequestedPrioReport | lsr:PaymentCat | lsr:Reservation))+1}">
                        <xsl:value-of select="substring(lsr:ReceiptDate/@V,1,10)"/>
                        <xsl:if test="contains(lsr:ReceiptDate/@V, 'T')">&#160;kl.<xsl:value-of select="substring(lsr:ReceiptDate/@V,12,5)"/>
                        </xsl:if>
                    </td>
                </xsl:if>
                <xsl:if test="lsr:RequestedPrioReport">
                    <th>Ønsket prioritet</th>
                    <td colspan="{(($col)-2-count(lsr:IssueDate | lsr:ReceiptDate)*2)*number(not(lsr:PaymentCat | lsr:Reservation))+1}">
                        <xsl:apply-templates select="lsr:RequestedPrioReport/@DN"/>
                    </td>
                </xsl:if>
                <xsl:if test="lsr:PaymentCat">
                    <th>Betalingskategori</th>
                    <td colspan="{(($col)-2-count(lsr:IssueDate | lsr:ReceiptDate | lsr:RequestedPrioReport)*2)*number(not(lsr:Reservation))+1}">
                        <xsl:value-of select="lsr:PaymentCat/@DN"/>
                    </td>
                </xsl:if>
                <xsl:if test="lsr:Reservation">
                    <th>Pasientens reservasjoner</th>
                    <td colspan="{($col)-1-count(lsr:IssueDate | lsr:ReceiptDate | lsr:RequestedPrioReport | lsr:PaymentCat)*2}">
                        <xsl:for-each select="lsr:Reservation">
                            <xsl:value-of select="@DN"/>
                            <xsl:if test="position()!=last()"><br/></xsl:if>
                        </xsl:for-each>
                    </td>
                </xsl:if>
            </tr>
        </xsl:if>
        <xsl:if test="lsr:ReqComment">
            <tr>
                <th>Rekvirentens kommentar</th>
                <td colspan="{($col)-1}">
                    <xsl:call-template name="line-breaks">
                        <xsl:with-param name="text" select="lsr:ReqComment"/>
                    </xsl:call-template>
                </td>
            </tr>
        </xsl:if>
        <xsl:if test="lsr:ReasonAsText">
            <tr>
                <th class="h2">Begrunnelser</th>
            </tr>
            <xsl:apply-templates select="lsr:ReasonAsText">
                <xsl:with-param name="col" select="$col"/>
            </xsl:apply-templates>
        </xsl:if>
        <xsl:if test="lsr:Comment">
            <tr>
                <th class="h2">Kommentarer</th>
            </tr>
            <xsl:apply-templates select="lsr:Comment">
                <xsl:with-param name="col" select="$col"/>
            </xsl:apply-templates>
        </xsl:if>
    </xsl:template>
    <!-- Visning av Kommentar til rekvisisjon -->
    <xsl:template match="lsr:Comment">
        <xsl:param name="col"/>
        <tr>
            <th>
                <xsl:choose>
                    <xsl:when test="lsr:Heading/@DN">
                        <xsl:value-of select="lsr:Heading/@DN"/>
                    </xsl:when>
                    <xsl:when test="lsr:Heading/@V">
                        <xsl:value-of select="lsr:Heading/@V"/>
                    </xsl:when>
                    <xsl:otherwise>
                        Ukjent overskrift&#160;
                    </xsl:otherwise>
                </xsl:choose>
            </th>
            <td colspan="{($col)-1}">
                <xsl:if test="lsr:TextCode">
                    <b>Kode:&#160;</b>
                    <xsl:value-of select="lsr:TextCode/@V"/> - <b>Kodetekst:&#160;</b>
                    <xsl:value-of select="lsr:TextCode/@DN"/>&#160;
                </xsl:if>
                <xsl:call-template name="line-breaks">
                    <xsl:with-param name="text" select="lsr:TextResultValue"/>
                </xsl:call-template>&#160;
            </td>
        </tr>
    </xsl:template>
    <!-- Visning av Begrunnelse for rekvisisjon -->
    <xsl:template match="lsr:ReasonAsText">
        <xsl:param name="col"/>
        <tr>
            <th>
                <xsl:choose>
                    <xsl:when test="lsr:Heading/@DN">
                        <xsl:value-of select="lsr:Heading/@DN"/>
                    </xsl:when>
                    <xsl:when test="lsr:Heading/@V">
                        <xsl:value-of select="lsr:Heading/@V"/>
                    </xsl:when>
                    <xsl:otherwise>
                        Heading&#160;
                    </xsl:otherwise>
                </xsl:choose>
            </th>
            <td colspan="{($col)-1}">
                <xsl:if test="lsr:TextCode">
                    <b>Kode:&#160;</b>
                    <xsl:value-of select="lsr:TextCode/@V"/> - <b>Kodetekst:&#160;</b>
                    <xsl:value-of select="lsr:TextCode/@DN"/>&#160;
                </xsl:if>
                <xsl:for-each select="lsr:TextResultValue">
                    <xsl:choose>
                        <xsl:when test="count(child::*)=0">
                            <xsl:call-template name="line-breaks">
                                <xsl:with-param name="text" select="."/>
                            </xsl:call-template>&#160;
                        </xsl:when>
                        <xsl:otherwise>
                            <xsl:copy-of select="node()"/>
                        </xsl:otherwise>
                    </xsl:choose>
                </xsl:for-each>
            </td>
        </tr>
    </xsl:template>
    <!-- Visning av Øvrig pasientinformasjon -->
    <xsl:template match="lsr:Patient">
        <xsl:param name="col"/>
        <tr>
            <th>Navn
                <xsl:if test="local-name(..)=&quot;Patient&quot;">-&#160;relasjon:&#160;<xsl:value-of select="lsr:Relation/@DN"/>
                </xsl:if>
            </th>
            <td>
                <xsl:value-of select="lsr:Name"/>
            </td>
            <th>
                <xsl:value-of select="lsr:TypeOffId/@V"/>
            </th>
            <td colspan="{(($col)-2-count(lsr:Name)*2)*number(not(lsr:Sex | lsr:DateOfBirth | lsr:DateOfDeath))+1}">
                <xsl:value-of select="lsr:OffId"/>
            </td>
            <xsl:if test="lsr:Sex">
                <th>Kjønn</th>
                <td colspan="{(($col)-2-count(lsr:Name | lsr:OffId)*2)*number(not(lsr:DateOfBirth | lsr:DateOfDeath))+1}">
                    <xsl:apply-templates select="lsr:Sex/@DN"/>
                </td>
            </xsl:if>
            <xsl:if test="lsr:DateOfBirth">
                <th>Fødselsdato</th>
                <td colspan="{(($col)-2-count(lsr:Name | lsr:OffId | lsr:Sex)*2)*number(not(lsr:DateOfDeath))+1}">
                    <xsl:value-of select="substring(lsr:DateOfBirth/@V,1,10)"/>
                    <xsl:if test="contains(lsr:DateOfBirth/@V, 'T')">&#160;kl.<xsl:value-of select="substring(lsr:DateOfBirth/@V,12,5)"/>
                    </xsl:if>
                </td>
            </xsl:if>
            <xsl:if test="lsr:DateOfDeath">
                <th>Dødsdato</th>
                <td colspan="{(($col)-1-count(lsr:Name | lsr:OffId | lsr:Sex | lsr:DateOfBirth)*2)}">
                    <xsl:value-of select="substring(lsr:DateOfDeath/@V,1,10)"/>
                    <xsl:if test="contains(lsr:DateOfDeath/@V, 'T')">&#160;kl.<xsl:value-of select="substring(lsr:DateOfDeath/@V,12,5)"/>
                    </xsl:if>
                </td>
            </xsl:if>
        </tr>
        <xsl:for-each select="lsr:Address">
            <xsl:variable name="antall-tel" select="count(lsr:TeleAddress)"/>
            <tr>
                <xsl:apply-templates select="lsr:TeleAddress">
                    <xsl:with-param name="col" select="(($col)-1)*number(not(lsr:Type and (lsr:PostalCode or lsr:City)))+1"/>
                </xsl:apply-templates>
                <xsl:if test="lsr:Type and (lsr:PostalCode or lsr:City)">
                    <xsl:apply-templates select=".">
                        <xsl:with-param name="col" select="($col)-1-($antall-tel)*2"/>
                    </xsl:apply-templates>
                </xsl:if>
            </tr>
        </xsl:for-each>
        <xsl:if test="lsr:AdditionalId or lsr:BasisForHealthServices">
            <xsl:variable name="antall-id" select="count(lsr:AdditionalId)"/>
            <tr>
                <xsl:choose>
                    <xsl:when test="lsr:AdditionalId and lsr:BasisForHealthServices/@DN">
                        <th colspan="2">Andre pasient-identifikatorer</th>
                        <xsl:for-each select="lsr:AdditionalId">
                            <xsl:apply-templates select=".">
                                <xsl:with-param name="col">
                                    <xsl:choose>
                                        <xsl:when test="position() = $antall-id">
                                            <xsl:value-of select="($col)-3-($antall-id)*2"/>
                                        </xsl:when>
                                        <xsl:otherwise>1</xsl:otherwise>
                                    </xsl:choose>
                                </xsl:with-param>
                            </xsl:apply-templates>
                        </xsl:for-each>
                        <th>Refusjonsgrunnlag</th>
                        <td>
                            <xsl:value-of select="lsr:BasisForHealthServices/@DN"/>
                        </td>
                    </xsl:when>
                    <xsl:when test="lsr:AdditionalId and not(lsr:BasisForHealthServices/@DN)">
                        <th colspan="2">Andre pasient-identifikatorer</th>
                        <xsl:for-each select="lsr:AdditionalId">
                            <xsl:apply-templates select=".">
                                <xsl:with-param name="col">
                                    <xsl:choose>
                                        <xsl:when test="position() = $antall-id">
                                            <xsl:value-of select="($col)-1-($antall-id)*2"/>
                                        </xsl:when>
                                        <xsl:otherwise>1</xsl:otherwise>
                                    </xsl:choose>
                                </xsl:with-param>
                            </xsl:apply-templates>
                        </xsl:for-each>
                    </xsl:when>
                    <xsl:otherwise>
                        <th>Refusjonsgrunnlag</th>
                        <td colspan="{($col)-1}">
                            <xsl:value-of select="lsr:BasisForHealthServices/@DN"/>
                        </td>
                    </xsl:otherwise>
                </xsl:choose>
            </tr>
        </xsl:if>
        <xsl:for-each select="lsr:AdmLocation">
            <tr>
                <th class="h2">
                    Opphold
                </th>
            </tr>
            <tr>
                <th>Institusjon</th>
                <td colspan="{(($col)-4)*number(not(lsr:AdmCat | lsr:StartDateTime | lsr:EndDateTime))+3}">
                    <!-- Gir her boksen en default kolonnebredde på 3 mot normalt 1 -->
                    <xsl:value-of select="lsr:Location/lsr:Inst/lsr:Name"/>
                    <xsl:if test="lsr:Location/lsr:Inst/lsr:Dept/lsr:Name">,&#160;avd.&#160;<xsl:value-of select="lsr:Location/lsr:Inst/lsr:Dept/lsr:Name"/>
                    </xsl:if>
                    <xsl:for-each select="lsr:Location/lsr:SubLocation">,&#160;<xsl:value-of select="lsr:Type/@DN"/>:&#160;<xsl:value-of select="lsr:Place"/>
                    </xsl:for-each>
                </td>
                <xsl:if test="lsr:AdmCat">
                    <th>Type&#160;tjeneste&#160;</th>
                    <td colspan="{(($col)-6)*number(not(lsr:StartDateTime | lsr:EndDateTime))+1}">
                        <xsl:value-of select="lsr:AdmCat/@DN"/>
                    </td>
                </xsl:if>
                <xsl:if test="lsr:StartDateTime">
                    <th>Innlagt&#160;dato&#160;</th>
                    <td colspan="{(($col)-6-count(lsr:AdmCat)*2)*number(not(lsr:EndDateTime))+1}">
                        <xsl:value-of select="substring(lsr:StartDateTime/@V,1,10)"/>
                        <xsl:if test="contains(lsr:StartDateTime/@V, 'T')">&#160;kl.<xsl:value-of select="substring(lsr:StartDateTime/@V,12,5)"/>
                        </xsl:if>
                    </td>
                </xsl:if>
                <xsl:if test="lsr:EndDateTime">
                    <th>Utskrevet&#160;dato&#160;</th>
                    <td colspan="{(($col)-5-count(lsr:AdmCat | lsr:StartDateTime)*2)}">
                        <xsl:value-of select="substring(lsr:EndDateTime/@V,1,10)"/>
                        <xsl:if test="contains(lsr:EndDateTime/@V, 'T')">&#160;kl.<xsl:value-of select="substring(lsr:EndDateTime/@V,12,5)"/>
                        </xsl:if>
                    </td>
                </xsl:if>
            </tr>
        </xsl:for-each>
        <xsl:for-each select="lsr:Patient | lsr:InfItem">
            <xsl:apply-templates select=".">
                <xsl:with-param name="col" select="$col"/>
            </xsl:apply-templates>
        </xsl:for-each>
    </xsl:template>
    <!-- Visning av Øvrig informasjon om dyret -->
    <xsl:template match="lsr:Animal">
        <xsl:param name="col"/>
        <tr>
            <xsl:if test="lsr:NameOwner">
                <th>Eier&#160;</th>
                <td colspan="{(($col)-2)*number(not(lsr:Name | lsr:Sex | lsr:Species))+1}">
                    <xsl:value-of select="lsr:NameOwner"/>
                </td>
            </xsl:if>
            <xsl:if test="lsr:Name">
                <th>Navn&#160;
                    <xsl:if test="local-name(..)=&quot;Animal&quot;">
                        -&#160;relasjon:&#160;<xsl:value-of select="lsr:Relation/@DN"/>
                    </xsl:if>
                </th>
                <td colspan="{(($col)-2-count(lsr:NameOwner)*2)*number(not(lsr:Sex | lsr:Species))+1}">
                    <xsl:value-of select="lsr:Name"/>
                </td>
            </xsl:if>
            <xsl:if test="lsr:Sex">
                <th>Kjønn</th>
                <td colspan="{(($col)-2-count(lsr:NameOwner | lsr:Name)*2)*number(not(lsr:Species))+1}">
                    <xsl:apply-templates select="lsr:Sex/@DN"/>
                </td>
            </xsl:if>
            <xsl:if test="lsr:Species">
                <th>Art</th>
                <td colspan="{($col)-1-count(lsr:NameOwner | lsr:Name | lsr:Sex)*2}">
                    <xsl:value-of select="lsr:Species"/>
                </td>
            </xsl:if>
        </tr>
        <xsl:if test="lsr:Animal">
            <xsl:apply-templates select="lsr:Animal">
                <xsl:with-param name="col" select="$col"/>
            </xsl:apply-templates>
        </xsl:if>
    </xsl:template>
    <!-- Visning av Analysert objekt -->
    <xsl:template match="lsr:AnalysedSubject">
        <xsl:param name="col"/>
        <tr>
            <th class="h2">
                <b>Prøve&#160;</b>
                <xsl:if test="lsr:ServType">
                    <xsl:choose>
                        <xsl:when test="lsr:ServType/@DN">,&#160;<xsl:value-of select="lsr:ServType/@DN"/>
                        </xsl:when>
                        <xsl:otherwise>,&#160;<xsl:value-of select="lsr:ServType/@V"/>
                        </xsl:otherwise>
                    </xsl:choose>
                </xsl:if>
            </th>
        </tr>
        <xsl:if test="lsr:IdByServProvider or lsr:Type or lsr:TypeCoded or lsr:CollectedSample or lsr:CollectedStudyProduct">
            <tr>
                <xsl:if test="lsr:IdByServProvider">
                    <th>Tjenesteyter ID</th>
                    <td colspan="{(($col)-2)*number(not(lsr:Type | lsr:TypeCoded | lsr:CollectedSample | lsr:CollectedStudyProduct))+1}">
                        <xsl:value-of select="lsr:IdByServProvider"/>
                    </td>
                </xsl:if>
                <xsl:if test="lsr:Type or lsr:TypeCoded">
                    <th>Type materiale</th>
                    <td colspan="{(($col)-2-count(lsr:IdByServProvider)*2)*number(not(lsr:CollectedSample | lsr:CollectedStudyProduct))+1}">
                        <xsl:choose>
                            <xsl:when test="lsr:Type">
                                <xsl:value-of select="lsr:Type"/>
                            </xsl:when>
                            <xsl:otherwise>
                                <xsl:choose>
                                    <xsl:when test="lsr:TypeCoded/@OT">
                                        <xsl:value-of select="lsr:TypeCoded/@OT"/>
                                    </xsl:when>
                                    <xsl:otherwise>
                                        <xsl:value-of select="lsr:TypeCoded/@DN"/>
                                    </xsl:otherwise>
                                </xsl:choose>
                            </xsl:otherwise>
                        </xsl:choose>
                    </td>
                </xsl:if>
                <xsl:for-each select="lsr:CollectedSample">
                    <xsl:if test="lsr:CollectedDate">
                        <th>Prøve tatt - dato</th>
                        <td colspan="{(($col)-2-number(boolean(../lsr:Type or ../lsr:TypeCoded))*2-count(../lsr:IdByServProvider)*2)*number(not(lsr:CollectorComment | lsr:CollectorCommentCoded | lsr:Logistics))+1}">
                            <xsl:value-of select="substring(lsr:CollectedDate/@V,1,10)"/>
                            <xsl:if test="contains(lsr:CollectedDate/@V, 'T')">&#160;kl.<xsl:value-of select="substring(lsr:CollectedDate/@V,12,5)"/>
                            </xsl:if>
                        </td>
                    </xsl:if>
                    <xsl:if test="lsr:Logistics">
                        <th>Forsendelsesmåte</th>
                        <td colspan="{(($col)-2-number(boolean(../lsr:Type or ../lsr:TypeCoded))*2-count(../lsr:IdByServProvider | lsr:CollectedDate)*2)*number(not(lsr:CollectorComment | lsr:CollectorCommentCoded))+1}">
                            <xsl:value-of select="lsr:Logistics"/>
                        </td>
                    </xsl:if>
                    <xsl:if test="lsr:CollectorComment or lsr:CollectorCommentCoded">
                        <th>Prøvetakers kommentar</th>
                        <td colspan="{($col)-1-number(boolean(../lsr:Type or ../lsr:TypeCoded))*2-count(../lsr:IdByServProvider | lsr:CollectedDate | lsr:Logistics)*2}">
                            <xsl:if test="lsr:CollectorComment">
                                <xsl:call-template name="line-breaks">
                                    <xsl:with-param name="text" select="lsr:CollectorComment"/>
                                </xsl:call-template>&#160;
                            </xsl:if>
                            <xsl:for-each select="lsr:CollectorCommentCoded">
                                <xsl:choose>
                                    <xsl:when test="@OT">
                                        <xsl:value-of select="@OT"/>&#160;</xsl:when>
                                    <xsl:otherwise>
                                        <xsl:value-of select="@DN"/>&#160;</xsl:otherwise>
                                </xsl:choose>
                            </xsl:for-each>
                        </td>
                    </xsl:if>
                </xsl:for-each>
                <xsl:for-each select="lsr:CollectedStudyProduct">
                    <xsl:if test="lsr:Type">
                        <th>Type analyseprodukt</th>
                        <td colspan="{(($col)-2-number(boolean(../lsr:Type or ../lsr:TypeCoded))*2-count(../lsr:IdByServProvider)*2)*number(not(lsr:ProducedDate | lsr:RefRelatedProd))+1}">
                            <xsl:value-of select="lsr:Type"/>
                        </td>
                    </xsl:if>
                    <xsl:if test="lsr:ProducedDate">
                        <th>Avledet prøve tatt - dato</th>
                        <td colspan="{(($col)-2-number(boolean(../lsr:Type or ../lsr:TypeCoded))*2-count(../lsr:IdByServProvider | lsr:Type)*2)*number(not(lsr:RefRelatedProd))+1}">
                            <xsl:value-of select="substring(lsr:ProducedDate/@V,1,10)"/>
                            <xsl:if test="contains(lsr:ProducedDate/@V, 'T')">&#160;kl.<xsl:value-of select="substring(lsr:ProducedDate/@V,12,5)"/>
                            </xsl:if>
                        </td>
                    </xsl:if>
                    <xsl:if test="lsr:RefRelatedProd">
                        <th>Referanse til relatert produkt</th>
                        <td colspan="{($col)-1-number(boolean(../lsr:Type or ../lsr:TypeCoded))*2-count(../lsr:IdByServProvider | lsr:Type | lsr:ProducedDate)*2}">
                            <xsl:value-of select="lsr:RefRelatedProd"/>
                        </td>
                    </xsl:if>
                </xsl:for-each>
            </tr>
        </xsl:if>
        <xsl:if test="lsr:IdByRequester or lsr:Number or lsr:SampleCollInd or lsr:SampleCollProc">
            <tr>
                <xsl:if test="lsr:IdByRequester">
                    <th>Rekvirent ID</th>
                    <td colspan="{(($col)-2)*number(not(lsr:Number | lsr:SampleCollInd | lsr:SampleCollProc))+1}">
                        <xsl:value-of select="lsr:IdByRequester"/>
                    </td>
                </xsl:if>
                <xsl:if test="lsr:Number">
                    <th>Antall</th>
                    <td colspan="{(($col)-2-count(lsr:IdByRequester)*2)*number(not(SampleCollInd | lsr:SampleCollProc))+1}">
                        <xsl:value-of select="lsr:Number"/>
                    </td>
                </xsl:if>
                <xsl:if test="lsr:SampleCollInd">
                    <th>Prøve tatt av</th>
                    <td colspan="{(($col)-2-count(lsr:IdByRequester | lsr:Number)*2)*number(not(lsr:SampleCollProc))+1}">
                        <xsl:choose>
                            <xsl:when test="lsr:SampleCollInd/@V=&quot;1&quot;">Rekvirent</xsl:when>
                            <xsl:otherwise>Tjenesteyter</xsl:otherwise>
                        </xsl:choose>
                    </td>
                </xsl:if>
                <xsl:if test="lsr:SampleCollProc">
                    <th>Prøvetakingsprosedyre</th>
                    <td colspan="{($col)-1-count(lsr:IdByRequester | lsr:Number | lsr:SampleCollInd)*2}">
                        <xsl:choose>
                            <xsl:when test="lsr:SampleCollProc/@OT">
                                <xsl:value-of select="lsr:SampleCollProc/@OT"/>
                            </xsl:when>
                            <xsl:otherwise>
                                <xsl:value-of select="lsr:SampleCollProc/@DN"/>
                            </xsl:otherwise>
                        </xsl:choose>
                    </td>
                </xsl:if>
            </tr>
        </xsl:if>
        <xsl:if test="lsr:AnatomicalOrigin or lsr:PreservMaterial or lsr:Pretreatment or lsr:Accredited or lsr:RelServProv">
            <tr>
                <xsl:if test="lsr:AnatomicalOrigin">
                    <th>Anatomisk lokasjon</th>
                    <td colspan="{(($col)-2)*number(not(lsr:PreservMaterial | lsr:Pretreatment | lsr:Accredited | lsr:RelServProv))+1}">
                        <xsl:value-of select="lsr:AnatomicalOrigin"/>
                    </td>
                </xsl:if>
                <xsl:if test="lsr:PreservMaterial">
                    <th>Konserveringsmiddel</th>
                    <td colspan="{(($col)-2-count(lsr:AnatomicalOrigin)*2)*number(not(lsr:Pretreatment | lsr:Accredited | lsr:RelServProv))+1}">
                        <xsl:value-of select="lsr:PreservMaterial"/>
                    </td>
                </xsl:if>
                <xsl:if test="lsr:Pretreatment">
                    <th>Forberedende behandling</th>
                    <td colspan="{(($col)-2-count(lsr:AnatomicalOrigin | lsr:PreservMaterial)*2)*number(not(lsr:Accredited | lsr:RelServProv))+1}">
                        <xsl:choose>
                            <xsl:when test="lsr:Pretreatment/lsr:Heading/@DN">
                                <xsl:value-of select="lsr:Pretreatment/lsr:Heading/@DN"/>&#160;</xsl:when>
                            <xsl:otherwise>
                                <b>Kodet:&#160;</b>
                                <xsl:value-of select="lsr:Pretreatment/lsr:Heading/@V"/>&#160;</xsl:otherwise>
                        </xsl:choose>
                        <xsl:if test="lsr:Pretreatment">
                            <b>Beskrivelse:&#160;</b>
                            <xsl:call-template name="line-breaks">
                                <xsl:with-param name="text" select="lsr:Pretreatment/lsr:TextResultValue"/>
                            </xsl:call-template>
                        </xsl:if>
                    </td>
                </xsl:if>
                <xsl:if test="lsr:Accredited">
                    <th>Akkreditert</th>
                    <td colspan="{(($col)-2-count(lsr:AnatomicalOrigin | lsr:PreservMaterial | lsr:Pretreatment)*2)*number(not(lsr:RelServProv))+1}">
                        <xsl:value-of select="lsr:Accredited/@V"/>
                    </td>
                </xsl:if>
                <xsl:if test="lsr:RelServProv">
                    <th>
                        <xsl:choose>
                            <xsl:when test="lsr:RelServProv/lsr:Relation/@DN">
                                <xsl:value-of select="lsr:RelServProv/lsr:Relation/@DN"/>
                            </xsl:when>
                            <xsl:otherwise>
                                Ansvarlig
                            </xsl:otherwise>
                        </xsl:choose>
                    </th>
                    <td colspan="{($col)-1-count(lsr:AnatomicalOrigin | lsr:PreservMaterial | lsr:Pretreatment | lsr:Accredited)*2}">
                        <xsl:value-of select="descendant::lsr:Name"/>
                    </td>
                </xsl:if>
            </tr>
        </xsl:if>
        <xsl:if test="lsr:SampleHandling">
            <tr>
                <th>Håndteringsbeskrivelse</th>
                <td colspan="{($col)-1}">
                    <xsl:for-each select="lsr:SampleHandling">
                        <xsl:call-template name="line-breaks">
                            <xsl:with-param name="text" select="."/>
                        </xsl:call-template>
                    </xsl:for-each>
                </td>
            </tr>
        </xsl:if>
        <xsl:if test="lsr:Comment">
            <tr>
                <th>Kommentar</th>
                <td colspan="{($col)-1}">
                    <xsl:call-template name="line-breaks">
                        <xsl:with-param name="text" select="lsr:Comment"/>
                    </xsl:call-template>
                </td>
            </tr>
        </xsl:if>
        <xsl:for-each select="lsr:AnalysedSubject">
            <xsl:apply-templates select=".">
                <xsl:with-param name="col" select="$col"/>
            </xsl:apply-templates>
        </xsl:for-each>
    </xsl:template>
    <!-- Visning av Undersøkelsesresultat -->
    <xsl:template match="lsr:ResultItem">
        <xsl:param name="col"/>
        <xsl:variable name="del-svar" select="local-name(..)=&quot;ResultItem&quot;"/>
        <tr>
            <xsl:choose>
                <xsl:when test="lsr:Investigation and not($del-svar)">
                    <td class="Emphasized" colspan="{$investigation-col}">
                        <xsl:for-each select="lsr:Investigation/lsr:Id">
                            <b>
                                <xsl:choose>
                                    <xsl:when test="@OT">
                                        <xsl:value-of select="@OT"/>&#160;</xsl:when>
                                    <xsl:otherwise>
                                        <xsl:value-of select="@DN"/>&#160;</xsl:otherwise>
                                </xsl:choose>
                            </b>
                        </xsl:for-each>
                        <xsl:if test="lsr:ServType">
                            <xsl:choose>
                                <xsl:when test="lsr:ServType/@DN">-&#160;<xsl:value-of select="lsr:ServType/@DN"/>
                                </xsl:when>
                                <xsl:otherwise>-&#160;<xsl:value-of select="lsr:ServType/@V"/>
                                </xsl:otherwise>
                            </xsl:choose>
                        </xsl:if>
                    </td>
                </xsl:when>
                <xsl:when test="lsr:Investigation and $del-svar">
                    <td colspan="{$investigation-col}">
                        <xsl:for-each select="lsr:Investigation/lsr:Id">
                            <xsl:choose>
                                <xsl:when test="@OT">
                                    <xsl:value-of select="@OT"/>&#160;</xsl:when>
                                <xsl:otherwise>
                                    <xsl:value-of select="@DN"/>&#160;</xsl:otherwise>
                            </xsl:choose>
                        </xsl:for-each>
                        <xsl:if test="lsr:ServType">
                            <xsl:choose>
                                <xsl:when test="lsr:ServType/@DN">-&#160;<xsl:value-of select="lsr:ServType/@DN"/>
                                </xsl:when>
                                <xsl:otherwise>-&#160;<xsl:value-of select="lsr:ServType/@V"/>
                                </xsl:otherwise>
                            </xsl:choose>
                        </xsl:if>
                    </td>
                </xsl:when>
                <xsl:otherwise>
                    <xsl:if test="../..//lsr:Investigation">
                        <td colspan="{$investigation-col}"/>
                    </xsl:if>
                </xsl:otherwise>
            </xsl:choose>
            <xsl:choose>
                <xsl:when test="lsr:TextResult or ../lsr:ResultItem/lsr:TextResult">
                    <td>
                        <xsl:if test="lsr:IdResultItem">(<xsl:value-of select="lsr:IdResultItem"/>)&#160;</xsl:if>
                        <xsl:choose>
                            <xsl:when test="lsr:TextResult/lsr:Heading/@DN">
                                <xsl:value-of select="lsr:TextResult/lsr:Heading/@DN"/>
                            </xsl:when>
                            <xsl:otherwise>
                                <xsl:value-of select="lsr:TextResult/lsr:Heading/@V"/>
                            </xsl:otherwise>
                        </xsl:choose>
                    </td>
                    <td colspan="{$textresult-col}">
                        <xsl:if test="lsr:RefIdResultItem">(<xsl:value-of select="lsr:RefIdResultItem"/>)&#160;</xsl:if>
                        <xsl:for-each select="lsr:TextResult/lsr:TextResultValue">
                            <xsl:choose>
                                <xsl:when test="count(child::*)=0">
                                    <xsl:call-template name="line-breaks">
                                        <xsl:with-param name="text" select="."/>
                                    </xsl:call-template>&#160;
                                </xsl:when>
                                <xsl:otherwise>
                                    <xsl:copy-of select="node()"/>
                                </xsl:otherwise>
                            </xsl:choose>
                        </xsl:for-each>
                        <xsl:for-each select="lsr:TextResult/lsr:TextCode">
                            <xsl:choose>
                                <xsl:when test="@OT">
                                    <xsl:value-of select="@OT"/>&#160;</xsl:when>
                                <xsl:otherwise>
                                    <xsl:value-of select="@DN"/>&#160;</xsl:otherwise>
                            </xsl:choose>
                        </xsl:for-each>
                        <xsl:if test="lsr:TextResult/lsr:Unit">
                            <b>Benevning:</b>&#160;<xsl:value-of select="lsr:TextResult/lsr:Unit"/>&#160;
                        </xsl:if>
                    </td>
                </xsl:when>
                <xsl:otherwise>
                    <xsl:if test="../..//lsr:TextResult">
                        <td/>
                        <td colspan="{$textresult-col}"/>
                    </xsl:if>
                </xsl:otherwise>
            </xsl:choose>
            <xsl:choose>
                <xsl:when test="lsr:Interval or ../lsr:ResultItem/lsr:Interval">
                    <td>
                        <xsl:if test="lsr:IdResultItem">(<xsl:value-of select="lsr:IdResultItem"/>)&#160;</xsl:if>
                        <xsl:choose>
                            <xsl:when test="lsr:Interval/lsr:Low">
                                <xsl:value-of select="lsr:Interval/lsr:Low/@U"/>
                            </xsl:when>
                            <xsl:otherwise>
                                <xsl:value-of select="lsr:Interval/lsr:High/@U"/>
                            </xsl:otherwise>
                        </xsl:choose>
                    </td>
                    <td colspan="{$interval-col}">
                        <xsl:if test="lsr:RefIdResultItem">(<xsl:value-of select="lsr:RefIdResultItem"/>)&#160;</xsl:if>
                        <xsl:value-of select="lsr:Interval/lsr:Low/@V"/> - <xsl:value-of select="lsr:Interval/lsr:High/@V"/>
                    </td>
                </xsl:when>
                <xsl:otherwise>
                    <xsl:if test="../..//lsr:Interval">
                        <td/>
                        <td colspan="{$interval-col}"/>
                    </xsl:if>
                </xsl:otherwise>
            </xsl:choose>
            <xsl:choose>
                <xsl:when test="lsr:DateResult">
                    <td colspan="{$dateresult-col}">
                        <xsl:if test="lsr:RefIdResultItem">(<xsl:value-of select="lsr:RefIdResultItem"/>)&#160;</xsl:if>
                        <xsl:value-of select="substring(lsr:DateResult/lsr:DateResultValue/@V,1,10)"/>
                        <xsl:if test="contains(lsr:DateResult/lsr:DateResultValue/@V, 'T')">&#160;kl.<xsl:value-of select="substring(lsr:DateResult/lsr:DateResultValue/@V,12,5)"/>
                        </xsl:if>
                    </td>
                </xsl:when>
                <xsl:otherwise>
                    <xsl:if test="../..//lsr:DateResult">
                        <td colspan="{$dateresult-col}"/>
                    </xsl:if>
                </xsl:otherwise>
            </xsl:choose>
            <xsl:choose>
                <xsl:when test="lsr:NumResult or ../lsr:ResultItem/lsr:NumResult">
                    <td>
                        <xsl:if test="lsr:IdResultItem">(<xsl:value-of select="lsr:IdResultItem"/>)&#160;</xsl:if>
                        <xsl:value-of select="lsr:NumResult/lsr:NumResultValue/@U"/>
                    </td>
                    <td colspan="{$numresult-col}">
                        <xsl:if test="lsr:RefIdResultItem">(<xsl:value-of select="lsr:RefIdResultItem"/>)&#160;</xsl:if>
                        <xsl:choose>
                            <xsl:when test="lsr:DevResultInd">
                                <em>
                                    <xsl:for-each select="lsr:NumResult/lsr:ArithmeticComp">
                                        <xsl:call-template name="k-8239"/>
                                    </xsl:for-each>
                                    <xsl:value-of select="lsr:NumResult/lsr:NumResultValue/@V"/>&#160;*
                                </em>
                            </xsl:when>
                            <xsl:otherwise>
                                <xsl:for-each select="lsr:NumResult/lsr:ArithmeticComp">
                                    <xsl:call-template name="k-8239"/>
                                </xsl:for-each>
                                <xsl:value-of select="lsr:NumResult/lsr:NumResultValue/@V"/>
                            </xsl:otherwise>
                        </xsl:choose>
                    </td>
                </xsl:when>
                <xsl:otherwise>
                    <xsl:if test="../..//lsr:NumResult">
                        <td/>
                        <td colspan="{$numresult-col}"/>
                    </xsl:if>
                </xsl:otherwise>
            </xsl:choose>
            <xsl:choose>
                <xsl:when test="lsr:RefInterval">
                    <td colspan="{$refinterval-col}">
                        <xsl:for-each select="lsr:RefInterval">
                            <xsl:value-of select="."/>
                        </xsl:for-each>
                    </td>
                </xsl:when>
                <xsl:otherwise>
                    <xsl:if test="../..//lsr:RefInterval">
                        <td colspan="{$refinterval-col}"/>
                    </xsl:if>
                </xsl:otherwise>
            </xsl:choose>
            <xsl:choose>
                <xsl:when test="lsr:DevResultInd">
                    <td colspan="{$devresultind-col}">
                        <xsl:for-each select="lsr:DevResultInd">
                            <xsl:call-template name="k-8244"/>
                        </xsl:for-each>
                    </td>
                </xsl:when>
                <xsl:otherwise>
                    <xsl:if test="../..//lsr:DevResultInd">
                        <td colspan="{$devresultind-col}"/>
                    </xsl:if>
                </xsl:otherwise>
            </xsl:choose>
            <xsl:choose>
                <xsl:when test="lsr:InvDate">
                    <td colspan="{$invdate-col}">
                        <xsl:value-of select="substring(lsr:InvDate/@V,1,10)"/>
                        <xsl:if test="contains(lsr:InvDate/@V, 'T')">&#160;kl.<xsl:value-of select="substring(lsr:InvDate/@V,12,5)"/>
                        </xsl:if>
                    </td>
                </xsl:when>
                <xsl:otherwise>
                    <xsl:if test="../..//lsr:InvDate">
                        <td colspan="{$invdate-col}"/>
                    </xsl:if>
                </xsl:otherwise>
            </xsl:choose>
            <xsl:choose>
                <xsl:when test="lsr:DescrDate">
                    <td colspan="{$descrdate-col}">
                        <xsl:value-of select="substring(lsr:DescrDate/@V,1,10)"/>
                        <xsl:if test="contains(lsr:DescrDate/@V, 'T')">&#160;kl.<xsl:value-of select="substring(lsr:DescrDate/@V,12,5)"/>
                        </xsl:if>
                    </td>
                </xsl:when>
                <xsl:otherwise>
                    <xsl:if test="../..//lsr:DescrDate">
                        <td colspan="{$descrdate-col}"/>
                    </xsl:if>
                </xsl:otherwise>
            </xsl:choose>
            <xsl:choose>
                <xsl:when test="lsr:CounterSignDate">
                    <td colspan="{countersign-col}">
                        <xsl:value-of select="substring(lsr:CounterSignDate/@V,1,10)"/>
                        <xsl:if test="contains(lsr:CounterSignDate/@V, 'T')">&#160;kl.<xsl:value-of select="substring(lsr:CounterSignDate/@V,12,5)"/>
                        </xsl:if>
                    </td>
                </xsl:when>
                <xsl:otherwise>
                    <xsl:if test="../..//lsr:CounterSignDate">
                        <td colspan="{countersign-col}"/>
                    </xsl:if>
                </xsl:otherwise>
            </xsl:choose>
            <xsl:choose>
                <xsl:when test="lsr:MedicalValidationDate">
                    <td colspan="{$medvaldate-col}">
                        <xsl:value-of select="substring(lsr:MedicalValidationDate/@V,1,10)"/>
                        <xsl:if test="contains(lsr:MedicalValidationDate/@V, 'T')">&#160;kl.<xsl:value-of select="substring(lsr:MedicalValidationDate/@V,12,5)"/>
                        </xsl:if>
                    </td>
                </xsl:when>
                <xsl:otherwise>
                    <xsl:if test="../..//lsr:MedicalValidationDate">
                        <td colspan="{$medvaldate-col}"/>
                    </xsl:if>
                </xsl:otherwise>
            </xsl:choose>
            <xsl:choose>
                <xsl:when test="lsr:Accredited">
                    <td colspan="{$accred-col}">
                        <xsl:value-of select="lsr:Accredited/@V"/>
                    </td>
                </xsl:when>
                <xsl:otherwise>
                    <xsl:if test="../..//lsr:ResultItem/lsr:Accredited">
                        <td colspan="{$accred-col}"/>
                    </xsl:if>
                </xsl:otherwise>
            </xsl:choose>
            <xsl:choose>
                <xsl:when test="lsr:StructuredInfo">
                    <td colspan="{$structured-col}">
                        <xsl:for-each select="lsr:StructuredInfo">
                            <table>
                                <tbody>
                                    <tr>
                                        <th colspan="2">
                                            <xsl:value-of select="lsr:Type/@DN"/>
                                        </th>
                                    </tr>
                                    <xsl:if test="lsr:TextInfo">
                                        <tr>
                                            <th width="{$structured-head-width}">Tekst</th>
                                            <td>
                                                <xsl:for-each select="lsr:TextInfo">
                                                    <xsl:if test="position()!=1">
                                                        <br/>
                                                    </xsl:if>
                                                    <xsl:call-template name="line-breaks">
                                                        <xsl:with-param name="text" select="lsr:Text"/>
                                                    </xsl:call-template>
                                                </xsl:for-each>
                                            </td>
                                        </tr>
                                    </xsl:if>
                                    <xsl:if test="lsr:IntegerInfo">
                                        <tr>
                                            <th width="{$structured-head-width}">Heltall</th>
                                            <td>
                                                <xsl:for-each select="lsr:IntegerInfo">
                                                    <xsl:if test="position()!=1">
                                                        <br/>
                                                    </xsl:if>
                                                    <xsl:value-of select="lsr:Integer"/>
                                                </xsl:for-each>
                                            </td>
                                        </tr>
                                    </xsl:if>
                                    <xsl:if test="lsr:PhysicalInfo">
                                        <tr>
                                            <th width="{$structured-head-width}">Fysisk størrelse</th>
                                            <td>
                                                <xsl:for-each select="lsr:PhysicalInfo">
                                                    <xsl:if test="position()!=1">
                                                        <br/>
                                                    </xsl:if>
                                                    <xsl:value-of select="lsr:Quantity/@V"/>
                                                    <xsl:value-of select="lsr:Quantity/@U"/>
                                                </xsl:for-each>
                                            </td>
                                        </tr>
                                    </xsl:if>
                                    <xsl:if test="lsr:CodedInfo">
                                        <tr>
                                            <th width="{$structured-head-width}">Kodet opplysning</th>
                                            <td>
                                                <xsl:for-each select="lsr:CodedInfo">
                                                    <xsl:if test="position()!=1">
                                                        <br/>
                                                    </xsl:if>
                                                    <xsl:value-of select="lsr:Code/@V"/>&#160;<xsl:value-of select="lsr:Code/@DN"/>
                                                </xsl:for-each>
                                            </td>
                                        </tr>
                                    </xsl:if>
                                    <xsl:if test="lsr:BooleanInfo">
                                        <tr>
                                            <th width="{$structured-head-width}">Boolsk opplysning</th>
                                            <td>
                                                <xsl:for-each select="lsr:BooleanInfo">
                                                    <xsl:if test="position()!=1">
                                                        <br/>
                                                    </xsl:if>
                                                    <xsl:value-of select="lsr:Flag/@V"/>
                                                </xsl:for-each>
                                            </td>
                                        </tr>
                                    </xsl:if>
                                </tbody>
                            </table>
                        </xsl:for-each>
                    </td>
                </xsl:when>
                <xsl:otherwise>
                    <xsl:if test="../..//lsr:StructuredInfo">
                        <td colspan="{$structured-col}"/>
                    </xsl:if>
                </xsl:otherwise>
            </xsl:choose>
            <xsl:choose>
                <xsl:when test="lsr:RelServProv">
                    <td colspan="{$relserv-col}">
                        <xsl:for-each select="lsr:RelServProv">
                            <xsl:choose>
                                <xsl:when test="lsr:Relation/@DN">
                                    <xsl:value-of select="lsr:Relation/@DN"/>:
                                </xsl:when>
                                <xsl:otherwise>
                                    <xsl:value-of select="lsr:Relation/@V"/>:
                                </xsl:otherwise>
                            </xsl:choose>
                            <xsl:value-of select="descendant::lsr:Name"/>
                            <xsl:if test="position()!=last()"><br/></xsl:if>
                        </xsl:for-each>
                    </td>
                </xsl:when>
                <xsl:otherwise>
                    <xsl:if test="../..//lsr:ResultItem/lsr:RelServProv">
                        <td colspan="{$relserv-col}"/>
                    </xsl:if>
                </xsl:otherwise>
            </xsl:choose>
            <xsl:choose>
                <xsl:when test="lsr:Investigation/lsr:Comment | lsr:Investigation/lsr:Spec | lsr:Comment |  lsr:DiagComment">
                    <td colspan="{$merknad-col}">
                        <xsl:variable name="resultspes_pos" select="count(ancestor::lsr:ServReport/descendant::lsr:Spec)-count(following-sibling::*/descendant::lsr:Spec)-count(parent::lsr:ResultItem/following-sibling::*/descendant::lsr:Spec)-count(child::lsr:ResultItem/descendant::lsr:Spec)"/>
                        <xsl:for-each select="lsr:Investigation/lsr:Spec">
                            <b>
                                <xsl:value-of select="($und-spes)"/>
                                <xsl:value-of select="($resultspes_pos)-last()+position()"/>)&#160;
                            </b>
                        </xsl:for-each>
                        <xsl:variable name="resultkomm_pos" select="count(ancestor::lsr:ServReport/descendant::lsr:Investigation[child::lsr:Comment])-count(following-sibling::*/descendant::lsr:Comment[parent::lsr:Investigation])-count(parent::lsr:ResultItem/following-sibling::*/descendant::lsr:Comment[parent::lsr:Investigation])-count(child::lsr:ResultItem/lsr:Investigation[child::lsr:Comment])"/>
                        <xsl:for-each select="lsr:Investigation/lsr:Comment">
                            <b>
                                <xsl:value-of select="($und-komm)"/>
                                <xsl:value-of select="($resultkomm_pos)-last()+position()"/>)&#160;</b>
                        </xsl:for-each>
                        <xsl:if test="lsr:Comment">
                            <b>
                                <xsl:value-of select="($res-komm)"/>
                                <xsl:value-of select="count(ancestor::lsr:ServReport/descendant::lsr:ResultItem[child::lsr:Comment])-count(following-sibling::*/descendant::lsr:Comment[parent::lsr:ResultItem])-count(parent::lsr:ResultItem/following-sibling::*/descendant::lsr:Comment[parent::lsr:ResultItem])-count(child::lsr:ResultItem[child::lsr:Comment])"/>)&#160;</b>
                        </xsl:if>
                        <xsl:if test="lsr:DiagComment">
                            <b>
                                <xsl:value-of select="($dia-komm)"/>
                                <xsl:value-of select="count(ancestor::lsr:ServReport/descendant::lsr:ResultItem[child::lsr:DiagComment])-count(following-sibling::*/descendant::lsr:DiagComment)-count(parent::lsr:ResultItem/following-sibling::*/descendant::lsr:DiagComment)-count(child::lsr:ResultItem[child::lsr:DiagComment])"/>)&#160;</b>
                        </xsl:if>
                    </td>
                </xsl:when>
                <xsl:otherwise>
                    <xsl:if test="../..//lsr:Investigation/lsr:Comment | ../..//lsr:Investigation/lsr:Spec | ../..//lsr:ResultItem/lsr:Comment | ../..//lsr:DiagComment">
                        <td colspan="{$merknad-col}"/>
                    </xsl:if>
                </xsl:otherwise>
            </xsl:choose>
        </tr>
    </xsl:template>
    <!-- Visning av Klinisk opplysning -->
    <xsl:template match="lsr:InfItem">
        <xsl:param name="col"/>
        <tr>
            <th class="h2">
                <xsl:choose>
                    <xsl:when test="lsr:Type/@DN">
                        <xsl:value-of select="lsr:Type/@DN"/>
                    </xsl:when>
                    <xsl:otherwise>Type klinisk opplysning kodet:&#160;<xsl:value-of select="lsr:Type/@V"/>
                    </xsl:otherwise>
                </xsl:choose>
            </th>
        </tr>
        <xsl:if test="lsr:Observation/lsr:Description or lsr:Observation/lsr:CodedDescr or lsr:StartDateTime or lsr:EndDateTime or lsr:OrgDate">
            <tr>
                <xsl:if test="lsr:Observation/lsr:Description">
                    <th>Beskrivelse</th>
                    <td colspan="{(($col)-2)*number(not(lsr:Observation/lsr:CodedDescr | lsr:StartDateTime | lsr:EndDateTime | lsr:OrgDate))+1}">
                        <xsl:for-each select="lsr:Observation/lsr:Description">
                            <xsl:choose>
                                <xsl:when test="count(child::*)=0">
                                    <xsl:call-template name="line-breaks">
                                        <xsl:with-param name="text" select="."/>
                                    </xsl:call-template>&#160;
                                </xsl:when>
                                <xsl:otherwise>
                                    <xsl:copy-of select="node()"/>
                                </xsl:otherwise>
                            </xsl:choose>
                        </xsl:for-each>
                    </td>
                </xsl:if>
                <xsl:if test="lsr:Observation/lsr:CodedDescr">
                    <th>Kodet beskrivelse</th>
                    <td colspan="{(($col)-2-count(lsr:Observation/lsr:Description)*2)*number(not(lsr:StartDateTime | lsr:EndDateTime | lsr:OrgDate))+1}">
                        <xsl:choose>
                            <xsl:when test="lsr:Observation/lsr:CodedDescr/@OT">
                                <xsl:value-of select="lsr:Observation/lsr:CodedDescr/@OT"/>
                            </xsl:when>
                            <xsl:otherwise>
                                <xsl:value-of select="lsr:Observation/lsr:CodedDescr/@DN"/>
                            </xsl:otherwise>
                        </xsl:choose>
                    </td>
                </xsl:if>
                <xsl:if test="lsr:StartDateTime">
                    <th>Start-dato</th>
                    <td colspan="{(($col)-2-count(lsr:Observation/lsr:Description | lsr:Observation/lsr:CodedDescr)*2)*number(not(lsr:EndDateTime | lsr:OrgDate))+1}">
                        <xsl:value-of select="substring(lsr:StartDateTime/@V,1,10)"/>
                        <xsl:if test="contains(lsr:StartDateTime/@V, 'T')">&#160;kl.<xsl:value-of select="substring(lsr:StartDateTime/@V,12,5)"/>
                        </xsl:if>
                    </td>
                </xsl:if>
                <xsl:if test="lsr:EndDateTime">
                    <th>Slutt-dato</th>
                    <td colspan="{(($col)-2-count(lsr:Observation/lsr:Description | lsr:Observation/lsr:CodedDescr | lsr:StartDateTime)*2)*number(not(lsr:OrgDate))+1}">
                        <xsl:value-of select="substring(lsr:EndDateTime/@V,1,10)"/>
                        <xsl:if test="contains(lsr:EndDateTime/@V, 'T')">&#160;kl.<xsl:value-of select="substring(lsr:EndDateTime/@V,12,5)"/>
                        </xsl:if>
                    </td>
                </xsl:if>
                <xsl:if test="lsr:OrgDate">
                    <th>Slutt-dato</th>
                    <td colspan="{($col)-1-count(lsr:Observation/lsr:Description | lsr:Observation/lsr:CodedDescr | lsr:StartDateTime | lsr:EndDateTime)*2}">
                        <xsl:value-of select="substring(lsr:OrgDate/@V,1,10)"/>
                        <xsl:if test="contains(lsr:OrgDate/@V, 'T')">&#160;kl.<xsl:value-of select="substring(lsr:OrgDate/@V,12,5)"/>
                        </xsl:if>
                    </td>
                </xsl:if>
            </tr>
        </xsl:if>
        <xsl:if test="lsr:Observation/lsr:Comment">
            <tr>
                <th>Kommentar</th>
                <td colspan="{($col)-1}">
                    <xsl:call-template name="line-breaks">
                        <xsl:with-param name="text" select="lsr:Observation/lsr:Comment"/>
                    </xsl:call-template>
                </td>
            </tr>
        </xsl:if>
    </xsl:template>
    <!-- Visning av Tjenesteyter -->
    <xsl:template match="lsr:ServProvider">
        <xsl:param name="col"/>
        <tr>
            <th class="h2">
                Tjenesteyter
                <xsl:if test="lsr:HCP/lsr:MedSpeciality/@DN"> - Spesialitet:&#160;<xsl:value-of select="lsr:HCP/lsr:MedSpeciality/@DN"/>
                </xsl:if>
            </th>
        </tr>
        <xsl:for-each select="lsr:HCP">
            <xsl:apply-templates select=".">
                <xsl:with-param name="col" select="$col"/>
            </xsl:apply-templates>
        </xsl:for-each>
    </xsl:template>
    <!-- Visning av Henvisende instans -->
    <xsl:template match="lsr:Requester">
        <xsl:param name="col"/>
        <tr>
            <th class="h2">
                Henvisende instans
                <xsl:if test="lsr:HCP/lsr:MedSpeciality/@DN"> - Spesialitet:&#160;<xsl:value-of select="lsr:HCP/lsr:MedSpeciality/@DN"/>
                </xsl:if>
            </th>
        </tr>
        <xsl:for-each select="lsr:HCP">
            <xsl:apply-templates select=".">
                <xsl:with-param name="col" select="$col"/>
            </xsl:apply-templates>
        </xsl:for-each>
    </xsl:template>
    <!-- Visning av Betalingsansvarlig -->
    <xsl:template match="lsr:PaymentResponsible">
        <xsl:param name="col"/>
        <tr>
            <th class="h2">
                Betalingsansvarlig
                <xsl:if test="lsr:TypeGuarantor"> - <xsl:value-of select="lsr:TypeGuarantor"/>
                </xsl:if>
            </th>
        </tr>
        <xsl:for-each select="lsr:HCP">
            <xsl:apply-templates select=".">
                <xsl:with-param name="col" select="$col"/>
            </xsl:apply-templates>
        </xsl:for-each>
    </xsl:template>
    <!-- Visning av Kopimottaker -->
    <xsl:template match="lsr:CopyDest">
        <xsl:param name="col"/>
        <tr>
            <th class="h2">
                Kopimottaker
                <xsl:if test="lsr:CopyDestRole/lsr:MsgType/@DN"> - kopi&#160;av:&#160;<xsl:value-of select="lsr:CopyDestRole/lsr:MsgType/@DN"/>
                </xsl:if>
            </th>
        </tr>
        <xsl:for-each select="lsr:HCP">
            <xsl:apply-templates select=".">
                <xsl:with-param name="col" select="$col"/>
            </xsl:apply-templates>
        </xsl:for-each>
    </xsl:template>
    <!-- Visning av Tilknyttet tjeneteyter -->
    <xsl:template match="lsr:RelServProv">
        <xsl:param name="col"/>
        <tr>
            <th class="h2">
                <xsl:choose>
                    <xsl:when test="lsr:Relation/@DN">
                        <xsl:value-of select="lsr:Relation/@DN"/>
                    </xsl:when>
                    <xsl:otherwise>Tilknyttet tjenesteyter</xsl:otherwise>
                </xsl:choose>
            </th>
        </tr>
        <xsl:for-each select="lsr:HCP">
            <xsl:apply-templates select=".">
                <xsl:with-param name="col" select="$col"/>
            </xsl:apply-templates>
        </xsl:for-each>
    </xsl:template>
    <!-- Visning av Ansvarlig Helsetjenesteenhet -->
    <xsl:template match="lsr:ResponsibleHcp">
        <xsl:param name="col"/>
        <tr>
            <th class="h2">
                <xsl:choose>
                    <xsl:when test="lsr:Relation/@DN">
                        <xsl:value-of select="lsr:Relation/@DN"/>
                    </xsl:when>
                    <xsl:otherwise>Ansvarlig&#160;helsetjenesteenhet</xsl:otherwise>
                </xsl:choose>
            </th>
        </tr>
        <xsl:for-each select="lsr:HCP">
            <xsl:apply-templates select=".">
                <xsl:with-param name="col" select="$col"/>
            </xsl:apply-templates>
        </xsl:for-each>
    </xsl:template>
    <!-- Visning av Oppholdsted -->
    <xsl:template match="lsr:AdmLocation">
        <xsl:param name="col"/>
        <tr>
            <th class="h2">
                Oppholdssted for pasient
                <xsl:if test="lsr:AdmCat/@DN"> - Type&#160;tjeneste:&#160;<xsl:value-of select="lsr:AdmCat/@DN"/>
                </xsl:if>
            </th>
        </tr>
        <xsl:for-each select="lsr:Location/lsr:Inst">
            <tr>
                <xsl:apply-templates select=".">
                    <xsl:with-param name="col" select="$col"/>
                </xsl:apply-templates>
            </tr>
        </xsl:for-each>
        <xsl:for-each select="lsr:Location/lsr:Address">
            <xsl:variable name="antall-tel" select="count(lsr:TeleAddress)"/>
            <tr>
                <xsl:apply-templates select="lsr:TeleAddress">
                    <xsl:with-param name="col" select="(($col)-1)*number(not(lsr:Type and (lsr:PostalCode or lsr:City)))+1"/>
                </xsl:apply-templates>
                <xsl:if test="lsr:Type and (lsr:PostalCode or lsr:City)">
                    <xsl:apply-templates select=".">
                        <xsl:with-param name="col" select="($col)-1-($antall-tel)*2"/>
                    </xsl:apply-templates>
                </xsl:if>
            </tr>
        </xsl:for-each>
    </xsl:template>
    <!-- Visning av Helsetjenesteenhet -->
    <xsl:template match="lsr:HCP">
        <xsl:param name="col"/>
        <xsl:for-each select="lsr:Inst">
            <tr>
                <xsl:apply-templates select=".">
                    <xsl:with-param name="col" select="$col"/>
                </xsl:apply-templates>
            </tr>
        </xsl:for-each>
        <xsl:for-each select="lsr:Inst/lsr:Dept">
            <tr>
                <xsl:apply-templates select=".">
                    <xsl:with-param name="col" select="$col"/>
                </xsl:apply-templates>
            </tr>
        </xsl:for-each>
        <xsl:for-each select="lsr:Inst/lsr:HCPerson">
            <tr>
                <xsl:apply-templates select=".">
                    <xsl:with-param name="col" select="$col"/>
                </xsl:apply-templates>
            </tr>
        </xsl:for-each>
        <xsl:for-each select="lsr:HCProf">
            <tr>
                <xsl:apply-templates select=".">
                    <xsl:with-param name="col" select="$col"/>
                </xsl:apply-templates>
            </tr>
        </xsl:for-each>
        <xsl:for-each select="lsr:Address">
            <xsl:variable name="antall-tel" select="count(lsr:TeleAddress)"/>
            <tr>
                <xsl:apply-templates select="lsr:TeleAddress">
                    <xsl:with-param name="col" select="(($col)-1)*number(not(lsr:Type and (lsr:PostalCode or lsr:City)))+1"/>
                </xsl:apply-templates>
                <xsl:if test="lsr:Type and (lsr:PostalCode or lsr:City)">
                    <xsl:apply-templates select=".">
                        <xsl:with-param name="col" select="($col)-1-($antall-tel)*2"/>
                    </xsl:apply-templates>
                </xsl:if>
            </tr>
        </xsl:for-each>
    </xsl:template>
    <!-- Visning av Person i helsevesenet -->
    <xsl:template match="lsr:HCProf">
        <xsl:param name="col"/>
        <th>Person&#160;-
            <xsl:choose>
                <xsl:when test="lsr:Type/@DN">
                    <xsl:value-of select="lsr:Type/@DN"/>
                </xsl:when>
                <xsl:otherwise>Navn</xsl:otherwise>
            </xsl:choose>
        </th>
        <td>
            <xsl:value-of select="lsr:Name"/>
        </td>
        <th>
            <xsl:choose>
                <xsl:when test="lsr:TypeId/@V">
                    <xsl:value-of select="lsr:TypeId/@V"/>
                </xsl:when>
                <xsl:otherwise>Id</xsl:otherwise>
            </xsl:choose>
        </th>
        <xsl:choose>
            <xsl:when test="lsr:AdditionalId">
                <td>
                    <xsl:value-of select="lsr:Id"/>
                </td>
            </xsl:when>
            <xsl:otherwise>
                <td colspan="{($col)-3}">
                    <xsl:value-of select="lsr:Id"/>
                </td>
            </xsl:otherwise>
        </xsl:choose>
        <xsl:if test="lsr:AdditionalId">
            <xsl:variable name="antall-id" select="count(lsr:AdditionalId)"/>
            <xsl:for-each select="lsr:AdditionalId">
                <xsl:apply-templates select=".">
                    <xsl:with-param name="col">
                        <xsl:choose>
                            <xsl:when test="position() = $antall-id">
                                <xsl:value-of select="($col)-3-($antall-id)*2"/>
                            </xsl:when>
                            <xsl:otherwise>1</xsl:otherwise>
                        </xsl:choose>
                    </xsl:with-param>
                </xsl:apply-templates>
            </xsl:for-each>
        </xsl:if>
    </xsl:template>
    <!-- Visning av Institusjon -->
    <xsl:template match="lsr:Inst">
        <xsl:param name="col"/>
        <th>Navn</th>
        <td>
            <xsl:value-of select="lsr:Name"/>
        </td>
        <th>
            <xsl:choose>
                <xsl:when test="lsr:TypeId/@V">
                    <xsl:value-of select="lsr:TypeId/@V"/>
                </xsl:when>
                <xsl:otherwise>Id</xsl:otherwise>
            </xsl:choose>
        </th>
        <xsl:choose>
            <xsl:when test="lsr:AdditionalId">
                <td>
                    <xsl:value-of select="lsr:Id"/>
                </td>
            </xsl:when>
            <xsl:otherwise>
                <td colspan="{($col)-3}">
                    <xsl:value-of select="lsr:Id"/>
                </td>
            </xsl:otherwise>
        </xsl:choose>
        <xsl:if test="lsr:AdditionalId">
            <xsl:variable name="antall-id" select="count(lsr:AdditionalId)"/>
            <xsl:for-each select="lsr:AdditionalId">
                <xsl:apply-templates select=".">
                    <xsl:with-param name="col">
                        <xsl:choose>
                            <xsl:when test="position() = $antall-id">
                                <xsl:value-of select="($col)-3-($antall-id)*2"/>
                            </xsl:when>
                            <xsl:otherwise>1</xsl:otherwise>
                        </xsl:choose>
                    </xsl:with-param>
                </xsl:apply-templates>
            </xsl:for-each>
        </xsl:if>
    </xsl:template>
    <!-- Visning av Avdeling -->
    <xsl:template match="lsr:Dept">
        <xsl:param name="col"/>
        <th>
            Avdeling&#160;-
            <xsl:choose>
                <xsl:when test="lsr:Type/@DN">
                    <xsl:value-of select="lsr:Type/@DN"/>
                </xsl:when>
                <xsl:otherwise>Navn</xsl:otherwise>
            </xsl:choose>
        </th>
        <td>
            <xsl:value-of select="lsr:Name"/>
        </td>
        <th>
            <xsl:choose>
                <xsl:when test="lsr:TypeId/@V">
                    <xsl:value-of select="lsr:TypeId/@V"/>
                </xsl:when>
                <xsl:otherwise>Id</xsl:otherwise>
            </xsl:choose>
        </th>
        <xsl:choose>
            <xsl:when test="lsr:AdditionalId">
                <td>
                    <xsl:value-of select="lsr:Id"/>
                </td>
            </xsl:when>
            <xsl:otherwise>
                <td colspan="{($col)-3}">
                    <xsl:value-of select="lsr:Id"/>
                </td>
            </xsl:otherwise>
        </xsl:choose>
        <xsl:if test="lsr:AdditionalId">
            <xsl:variable name="antall-id" select="count(lsr:AdditionalId)"/>
            <xsl:for-each select="lsr:AdditionalId">
                <xsl:apply-templates select=".">
                    <xsl:with-param name="col">
                        <xsl:choose>
                            <xsl:when test="position() = $antall-id">
                                <xsl:value-of select="($col)-3-($antall-id)*2"/>
                            </xsl:when>
                            <xsl:otherwise>1</xsl:otherwise>
                        </xsl:choose>
                    </xsl:with-param>
                </xsl:apply-templates>
            </xsl:for-each>
        </xsl:if>
    </xsl:template>
    <!-- Visning av Person i organisasjon -->
    <xsl:template match="lsr:HCPerson">
        <xsl:param name="col"/>
        <th>Personnavn</th>
        <td>
            <xsl:value-of select="lsr:Name"/>
        </td>
        <th>
            <xsl:choose>
                <xsl:when test="lsr:TypeId/@V">
                    <xsl:value-of select="lsr:TypeId/@V"/>
                </xsl:when>
                <xsl:otherwise>Id</xsl:otherwise>
            </xsl:choose>
        </th>
        <xsl:choose>
            <xsl:when test="lsr:AdditionalId">
                <td>
                    <xsl:value-of select="lsr:Id"/>
                </td>
            </xsl:when>
            <xsl:otherwise>
                <td colspan="{($col)-3}">
                    <xsl:value-of select="lsr:Id"/>
                </td>
            </xsl:otherwise>
        </xsl:choose>
        <xsl:if test="lsr:AdditionalId">
            <xsl:variable name="antall-id" select="count(lsr:AdditionalId)"/>
            <xsl:for-each select="lsr:AdditionalId">
                <xsl:apply-templates select=".">
                    <xsl:with-param name="col">
                        <xsl:choose>
                            <xsl:when test="position() = $antall-id">
                                <xsl:value-of select="($col)-3-($antall-id)*2"/>
                            </xsl:when>
                            <xsl:otherwise>1</xsl:otherwise>
                        </xsl:choose>
                    </xsl:with-param>
                </xsl:apply-templates>
            </xsl:for-each>
        </xsl:if>
    </xsl:template>
    <!-- Visning av Alternativ id -->
    <xsl:template match="lsr:AdditionalId">
        <xsl:param name="col"/>
        <th>
            <xsl:value-of select="lsr:Type/@V"/>
        </th>
        <td colspan="{$col}">
            <xsl:value-of select="lsr:Id"/>
        </td>
    </xsl:template>
    <!-- Visning av Adresse -->
    <xsl:template match="lsr:Address">
        <xsl:param name="col"/>
        <th>
            <xsl:choose>
                <xsl:when test="lsr:Type/@DN">
                    <xsl:value-of select="lsr:Type/@DN"/>
                </xsl:when>
                <xsl:otherwise>Adresse</xsl:otherwise>
            </xsl:choose>
        </th>
        <td colspan="{$col}">
            <xsl:if test="lsr:StreetAdr">
                <xsl:value-of select="lsr:StreetAdr"/>,&#160;</xsl:if>
            <xsl:if test="lsr:PostalCode">
                <xsl:value-of select="lsr:PostalCode"/>
            </xsl:if>
            <xsl:if test="lsr:City">&#160;<xsl:value-of select="lsr:City"/>
            </xsl:if>
            <xsl:if test="lsr:CityDistr">,&#160;<xsl:value-of select="lsr:CityDistr/@DN"/>
            </xsl:if>
            <xsl:if test="lsr:County">,&#160;<xsl:value-of select="lsr:County/@DN"/>
            </xsl:if>
            <xsl:if test="lsr:Country">,&#160;<xsl:value-of select="lsr:Country/@DN"/>
            </xsl:if>
        </td>
    </xsl:template>
    <!-- Visning av Telekommunikasjon -->
    <xsl:template match="lsr:TeleAddress">
        <xsl:param name="col"/>
        <th>
            <xsl:choose>
                <xsl:when test="starts-with(@V, &quot;tel:&quot;) or starts-with(@V, &quot;callto:&quot;)">Telefon</xsl:when>
                <xsl:when test="starts-with(@V, &quot;fax:&quot;)">Faks</xsl:when>
                <xsl:when test="starts-with(@V, &quot;mailto:&quot;)">e-post</xsl:when>
                <xsl:otherwise>
                    <xsl:value-of select="substring-before(@V, &quot;:&quot;)"/>
                </xsl:otherwise>
            </xsl:choose>
        </th>
        <td colspan="{$col}">
            <xsl:value-of select="substring-after(@V, &quot;:&quot;)"/>
        </td>
    </xsl:template>
    <!-- Visning av Referert dokument -->
    <xsl:template match="lsr:RefDoc">
        <xsl:param name="col"/>
        <th class="h2">
            Vedlegg&#160;
        </th>
        <xsl:if test="lsr:MsgType or lsr:Id or lsr:IssueDate or lsr:MimeType or lsr:Compression">
            <tr>
                <xsl:if test="lsr:MsgType">
                    <th>Type</th>
                    <td colspan="{(($col)-2)*number(not(lsr:Id | lsr:IssueDate | lsr:MimeType | lsr:Compression))+1}">
                        <xsl:choose>
                            <xsl:when test="lsr:MsgType/@DN"><xsl:value-of select="lsr:MsgType/@DN"/>
                            </xsl:when>
                            <xsl:otherwise><b>Kodet:</b>
                                <xsl:value-of select="lsr:MsgType/@V"/>
                            </xsl:otherwise>
                        </xsl:choose>
                    </td>
                </xsl:if>
                <xsl:if test="lsr:Id">
                    <th>Id</th>
                    <td colspan="{(($col)-2-count(lsr:MsgType)*2)*number(not(IssueDate | lsr:MimeType | lsr:Compression))+1}">
                        <xsl:value-of select="lsr:Id"/>
                    </td>
                </xsl:if>
                <xsl:if test="lsr:IssueDate">
                    <th>Utstedt-dato</th>
                    <td colspan="{(($col)-2-count(lsr:MsgType | lsr:Id)*2)*number(not(lsr:MimeType | lsr:Compression))+1}">
                        <xsl:value-of select="substring(lsr:IssueDate/@V,1,10)"/>
                        <xsl:if test="contains(lsr:IssueDate/@V, 'T')">&#160;kl.<xsl:value-of select="substring(lsr:IssueDate/@V,12,5)"/>
                        </xsl:if>
                    </td>
                </xsl:if>
                <xsl:if test="lsr:MimeType">
                    <th>Mimetype</th>
                    <td colspan="{(($col)-2-count(lsr:MsgType | lsr:Id | lsr:IssueDate)*2)*number(not(lsr:Compression))+1}">
                        <xsl:value-of select="lsr:MimeType"/>
                    </td>
                </xsl:if>
                <xsl:if test="lsr:Compression">
                    <th>Komprimering</th>
                    <td colspan="{($col)-1-count(lsr:MsgType | lsr:Id | lsr:IssueDate | lsr:MimeType)*2}">
                        <xsl:choose>
                            <xsl:when test="lsr:Compression/@DN"><xsl:value-of select="lsr:Compression/@DN"/>
                            </xsl:when>
                            <xsl:otherwise><b>Kodet:</b>
                                <xsl:value-of select="lsr:Compression/@V"/>
                            </xsl:otherwise>
                        </xsl:choose>
                    </td>
                </xsl:if>
            </tr>
        </xsl:if>
        <xsl:if test="lsr:Description">
            <tr>
                <th>Beskrivelse</th>
                <td colspan="{($col)-1}">
                    <xsl:call-template name="line-breaks"><xsl:with-param name="text" select="lsr:Description"/></xsl:call-template>
                </td>
            </tr>
        </xsl:if>
        <xsl:if test="lsr:Content or lsr:FileReference">
            <tr>
                <xsl:for-each select="lsr:FileReference">
                    <th>Filreferanse</th>
                    <td colspan="{($col)-1}">
                        <xsl:value-of select="."/>
                    </td>
                </xsl:for-each>
                <xsl:for-each select="lsr:Content">
                    <th>Innhold</th>
                    <td colspan="{($col)-1}">
                        <xsl:value-of select="."/>
                    </td>
                </xsl:for-each>
            </tr>
        </xsl:if>
    </xsl:template>
    <!-- Hoveddokument slutt -->
    <!-- Funksjoner -->
    <!-- Funksjon for å få til linjeskift - for bruk ved datatypen ST eller string -->
    <xsl:template name="line-breaks">
        <xsl:param name="text"/>
        <xsl:choose>
            <xsl:when test="contains($text,'&#10;')">
                <xsl:value-of select="substring-before($text,'&#10;')"/>
                <br/>
                <xsl:call-template name="line-breaks">
                    <xsl:with-param name="text" select="substring-after($text,'&#10;')"/>
                </xsl:call-template>
            </xsl:when>
            <xsl:otherwise>
                <xsl:value-of select="$text"/>
            </xsl:otherwise>
        </xsl:choose>
    </xsl:template>
    <!-- Kodeverk -->
    <xsl:template name="k-8239">
        <xsl:choose>
            <xsl:when test="@V='GE'">&gt;=</xsl:when>
            <xsl:when test="@V='GT'">&gt;</xsl:when>
            <xsl:when test="@V='LE'">&lt;=</xsl:when>
            <xsl:when test="@V='LT'">&lt;</xsl:when>
            <xsl:when test="@V='MG'">&gt;&gt;</xsl:when>
            <xsl:when test="@V='ML'">&lt;&lt;</xsl:when>
            <xsl:when test="@V='NE'">!=</xsl:when>
        </xsl:choose>
    </xsl:template>
    <xsl:template name="k-8244">
        <xsl:choose>
            <xsl:when test="@V='1'">Over øvre referansegrense</xsl:when>
            <xsl:when test="@V='2'">Under nedre referansegrense</xsl:when>
            <xsl:when test="@V='3'">Utenfor referansegrensene</xsl:when>
        </xsl:choose>
    </xsl:template>
</xsl:stylesheet>
