<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                xmlns:lsr="http://www.kith.no/xmlstds/labsvar/2004-09-14" xmlns="http://www.w3.org/1999/xhtml">
    <xsl:output method="xml" version="1.0" encoding="UTF-8" indent="yes" omit-xml-declaration="yes" doctype-public="-//W3C//DTD XHTML 1.0 Strict//EN" doctype-system="http://www.w3.org/TR/xhtml1/DTD/xhtml1-strict.dtd"/>
    <!--
FORMÅL
XSLT for generering av html fra Svarrapport Medisinske Tjenester - Radiologi v.1.2

ENDRINGER:
08.08.2013: Tilpasset DIPS: Inkluderer ServReq/ReasonAsText som egen div hvis tilstede i input XML.
30.06.2008: Første versjon

Svakheter:
* CodedComment og andre kode verdier som ikke er fastsatt i IG men baserer seg på eksterne kodeverk vil ikke vises med mindre teksten er med i DN-feltet
* Ved nøstede Resultitems brukes bare feltene for "Investigation" , "StatusInvestigation" og "InvDate"for de nøstede ResultItems. Informasjon om tilknyttet tjenestyter,
  beskrivelsesdato (Svar skrevet) og beskrivelse hentes bare fra ResultItem på øverste nivå.
* Investigations må ligge under enten øverste ResultItem eller (ekslusivt eller) under nøstede ResultItem. De kan ikke finnes samtidig på begge steder.
* For nøstede ResultItems forutsettes det at det enten finnes en undersøkelsesdato for enten alle eller ingen av undersøkelsene for at rapporten skal
  kunne sammenstille undersøkelse og undersøkelsesdato korrekt.
* For kommentarer til rekvisisjonen er det valgt Comment/TextResultValue. Det finnes imidlertid flere kommentarfelt som er mulig å bruke for en rekvisisjon som
  det ikke er tatt hensyn til.
* Eventuelle kliniske opplysinger som ligger som ServReq/ReasonAsText skrives ikke ut i svarrapporten

-->
    <xsl:include href="../felleskomponenter/header_medisinske_tjenester.xsl"/>
    <xsl:template match="/">
        <html xmlns="http://www.w3.org/1999/xhtml">
            <head>
                <title>Svarrapport Radiologi</title>
                <meta http-equiv="content-type" content="text/html; charset=utf-8"/>
                <style type="text/css">

                    .Header
                    {
                        width: 95%;
                        margin: 1px;
                        border:1px solid gray;
                        padding:1em;
                        background-color: #ffffff;
                        font-family: arial;
                        font-size: 0.8em;
                    }
                    .Patient,.ServProvider,.Requester, .CopyDest
                    {
                        width: 45%;
                        height: 10em;
                        float: left;
                        border: solid black 1px;
                        padding: 0.5em;
                        margin: 0.5em;
                    }
                    .Results
                    {
                        width: 95%;
                        float: left;

                        padding: 0.5em;

                    }
                    .Document
                    {
                        width:95%;
                        clear:left;
                        margin: 1px;
                        border:1px solid gray;
                        padding:1em;
                        background-color: #ffffff;
                        font-family: arial;
                        font-size: 0.8em;
                    }


                    .Document h1
                    {
                        padding:0.3em;
                        color:white;
                        background-color:gray;
                        border:1px solid gray;
                        font: 1.5em;
                        line-height:0.7em;
                        margin-bottom:0.3em;
                    }

                    .Document h2
                    {
                        padding:0.5em;
                        color:white;
                        background-color: gray;
                        border:1px solid gray;
                        font-size: 1.1em;
                        line-height:0.7em;
                        margin-bottom:0.3em;
                    }

                    .Document h3
                    {
                        padding:0.5em;
                        color:black;
                        font-size: 1em;
                        line-height:0.7em;
                        margin-bottom:0.3em
                    }

                    table.Default
                    {
                        width:95%;
                        border:1px solid black;
                    }


                    .Document th
                    {
                        background-color: #CAE8EA;
                        border-style:none;
                        font-family:Arial;
                        font-size:12px;
                        text-align: left;
                        text-valign: top;
                    }

                    .Document td
                    {

                        background-color: white;
                        border-style: none;
                        text-align: left;
                        font-size: 12px;
                    }

                    table.Noborder
                    {
                        background-color:#FFFFFF;
                        width:95%
                        border:0;
                    }

                    th.Noborder
                    {
                        background-color:#FFFFFF;
                        border-style:none;
                        font-family:Arial;
                        font-size:12px;
                        text-align: left;
                        text-valign: top;
                    }

                    td.Noborder
                    {
                        background-color:#FFFFFF;
                        border-style: none;
                        text-align: left;
                        font-size: 12px;
                    }

                    td.Emphasized
                    {
                        background-color:#CAE8EA;
                    }

                    .Document p
                    {
                        margin-top:0.5em;
                    }
                    .TableFormHeader
                    {
                        background-color: #fff2ba;
                        text-align: left;
                        font-family: Arial;
                        border-style: none;
                        font-weight: bold;
                        font-size: 12px;
                    }

                    .DocumentFooter
                    {
                        padding:0.5em;
                        color:black;
                        background-color: #dddddd;
                        border:1px solid black;
                        font-size: 10px;
                        margin:0.5em;
                    }
                </style>
            </head>
            <body>
                <xsl:apply-templates/>
            </body>
        </html>
    </xsl:template>
    <!--Start på prøveheader-->
    <xsl:template match="lsr:ServReport" name="AnalysisHeader">
        <div class="Results">
            <b>Id til svarrapporten:  <xsl:value-of select="./lsr:ServProvId"/>
            </b>
            <br/>
            <b>Id til rekvisisjonen:  <xsl:value-of select="./lsr:ServReq/lsr:Id"/>
            </b>
            <br/>
            <b>Rekvisisjonen er mottatt:  <xsl:value-of select="./lsr:ServReq/lsr:ReceiptDate/@V"/>
            </b>
            <br/>
            <b>Prøve:  <xsl:value-of select="./lsr:MsgDescr/@DN"/>
            </b>
            <br/>
            <b>Status: <xsl:value-of select="//lsr:Status/@DN"/>
            </b>
            <br/>
            <xsl:if test="../lsr:ServReport/lsr:CodedComment">
                <b>Kommentar til rapporten: </b>
                <xsl:value-of select="../lsr:ServReport/lsr:CodedComment/@DN"/>
                <br/>
            </xsl:if>
            <xsl:if test="../lsr:ServReport/lsr:Comment">
                <b>Kommentar til rapporten: </b>
                <xsl:value-of select="../lsr:ServReport/lsr:Comment"/>
                <br/>
            </xsl:if>
            <xsl:if test="//lsr:Comment/lsr:TextResultValue">
                <b>Kommentar til rekvisisjonen: </b>
                <xsl:value-of select="//lsr:Comment/lsr:TextResultValue"/>
            </xsl:if>
            <table width="95%" class="Default">
                <tbody>
                    <xsl:for-each select="./lsr:Patient/lsr:ResultItem">
                        <tr>
                            <th class="Emphasized">Undersøkelse gjennomført:</th>
                            <th class="Emphasized">
                                Undersøkelsesdato:</th>
                            <th class="Emphasized">
                                Status:</th>
                        </tr>
                        <tr>
                            <td class="Noborder">
                                <xsl:for-each select=".//lsr:Investigation">
                                    <xsl:value-of select="lsr:Id/@DN"/>
                                    <xsl:text> (Norako: </xsl:text>
                                    <xsl:value-of select="lsr:Id/@V"/>
                                    <xsl:text>)</xsl:text>
                                    <br/>
                                </xsl:for-each>
                            </td>
                            <td class="Noborder">
                                <xsl:for-each select=".//lsr:InvDate">
                                    <xsl:value-of select="./@V"/>
                                    <br/>
                                </xsl:for-each>
                            </td>
                            <td class="Noborder">
                                <xsl:for-each select=".//lsr:Investigation">
                                    <xsl:value-of select="following-sibling::lsr:StatusInvestigation/@DN"/>
                                    <xsl:value-of select="preceding-sibling::lsr:StatusInvestigation/@DN"/>
                                    <br/>
                                </xsl:for-each>
                            </td>
                        </tr>
                        <br/>
                        <tr>
                            <th class="Noborder">
                                <b>Tilknyttet tjenesteyter:</b>
                            </th>
                            <th class="Noborder">
                                <b>Svar skrevet:</b>
                            </th>
                        </tr>
                        <tr>
                            <td class="Noborder">
                                <xsl:value-of select="lsr:RelServProv/lsr:Relation/@DN"/>: <xsl:value-of select="lsr:RelServProv/lsr:HCP/lsr:HCProf/lsr:Name"/>
                            </td>
                            <td class="Noborder">
                                <xsl:value-of select="lsr:DescrDate/@V"/>
                            </td>
                        </tr>
                    </xsl:for-each>
                </tbody>
            </table>
        </div>
    </xsl:template>
    <!--Slutt

  header -->
    <!--Start på svarbolk -->
    <xsl:template name="ResultBody">
        <div class="Results">
            <xsl:for-each select="./lsr:Patient/lsr:ResultItem">
                <table width="95%" class="Default">
                    <tbody>
                        <br/>
                        <tr>
                            <th class="Noborder"/>
                        </tr>
                        <tr>
                            <th class="Emphasized">Undersøkelse:</th>
                            <td class="Noborder">
                                <!-- Må skrive XPath uttrykk .// for å få ramset opp Investigation i enten øverste Resultitems eller alle Investigations i nøstede Resultitems
                          XPath uttrykk // gir feil ved at flere resultiems på samme nivå (altså flere uavhengige undersøkelser) repeteres (gir feil i Case 1)  -->
                                <xsl:for-each select=".//lsr:Investigation">
                                    <xsl:value-of select="lsr:Id/@DN"/>
                                    <br/>
                                </xsl:for-each>
                            </td>
                        </tr>
                        <tr>
                            <th class="Emphasized">Resultat:</th>
                            <td class="Noborder">
                                <b>
                                    <xsl:value-of select="lsr:TextResult/lsr:TextResultValue"/>
                                    <xsl:if test="not(lsr:TextResult/lsr:TextResultValue)">
                                        <xsl:value-of select="lsr:TextResult/lsr:TextCode/@DN"/>
                                    </xsl:if>
                                </b>
                            </td>
                        </tr>
                        <xsl:if test="./lsr:Comment">
                            <tr>
                                <th class="Emphasized">Kommentar:</th>
                                <td class="Noborder">
                                    <xsl:value-of select="./lsr:Comment"/>
                                </td>
                            </tr>
                        </xsl:if>
                    </tbody>
                </table>
            </xsl:for-each>
        </div>
    </xsl:template>
    <!-- Slutt på svarbolk -->
    <xsl:template name="ReasonTextSection">
        <xsl:if test="./lsr:ServReq/lsr:ReasonAsText">
            <div class="Document">
                <div class="Results">
                    <table width="95%" class="Default">
                        <tbody>
                            <tr>
                                <th class="Emphasized">
                                    <xsl:value-of select="./lsr:ServReq/lsr:ReasonAsText/lsr:Heading/@DN"/>
                                </th>
                            </tr>
                            <tr>
                                <td class="Noborder">
                                    <xsl:value-of select="./lsr:ServReq/lsr:ReasonAsText/lsr:TextResultValue"/>
                                </td>
                            </tr>
                        </tbody>
                    </table>
                </div>
                <div style="clear:both"/>
            </div>
        </xsl:if>
    </xsl:template>
    <xsl:template match="lsr:Message">
        <!-- utelater meldingsid og kommunikasjonsinformasjon -->
        <xsl:apply-templates select="lsr:ServReport"/>
    </xsl:template>
    <!-- Hoveddel dokument -->
    <xsl:template match="lsr:ServReport">
        <xsl:call-template name="Header"/>
        <div class="Document">
            <!--kaller opp prøveheader -->
            <xsl:call-template name="AnalysisHeader"/>
            <!--kaller opp svarbolk-->
        </div>
        <div class="Document">
            <xsl:call-template name="ResultBody"/>
        </div>
        <xsl:call-template name="ReasonTextSection"/>
    </xsl:template>
    <!-- Slutt på hoveddel dokument -->
</xsl:stylesheet>