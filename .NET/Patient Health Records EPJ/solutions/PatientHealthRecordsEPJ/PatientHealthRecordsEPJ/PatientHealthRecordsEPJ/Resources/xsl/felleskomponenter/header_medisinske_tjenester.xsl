<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:lsr="http://www.kith.no/xmlstds/labsvar/2004-09-14" xmlns="http://www.w3.org/1999/xhtml" xmlns:xhtml="http://www.w3.org/1999/xhtml" exclude-result-prefixes="lsr xhtml">
    <!--
FORMÅL
Felles XSLT for generering av html for visningsfiler for diverse svarrapporter av Medisinske Tjenester - v1.0

ENDRINGER:
23.05.2008
-->

    <!-- Meldingshodet - avsender og mottaker-informasjon -->
    <xsl:template name="Header">
        <div class="Header">
            <div class="Patient">
                <b>Pasient:</b>
                <br/>
                <xsl:apply-templates select="lsr:Patient"/>
            </div>
            <div class="ServProvider">
                <b>Tjenesteyter:</b>
                <br/>
                <xsl:apply-templates select="lsr:ServProvider"/>
                <xsl:apply-templates select="lsr:RelServProv"/>
            </div>
            <div class="Requester">
                <b>Rekvirent:</b>
                <br/>
                <xsl:apply-templates select="lsr:Requester"/>
            </div>
            <div class="CopyDest">
                <b>Kopimottaker(e)</b>
                <br/>
                <xsl:apply-templates select="lsr:CopyDest"/>
            </div>
        </div>
    </xsl:template>


    <xsl:template match="lsr:Patient">
        <b>Navn:&#160;</b>
        <xsl:value-of select="lsr:Name"/>
        <br/>
        <xsl:if test="lsr:OffId">
            <b>
                <xsl:value-of select="lsr:TypeOffId/@DN"/>:&#160;</b>
            <xsl:value-of select="lsr:OffId"/>
        </xsl:if>
        <br/>
        <xsl:if test="lsr:Address">
            <xsl:apply-templates select="lsr:Address"/>
        </xsl:if>
    </xsl:template>



    <xsl:template match="lsr:Address">
        <xsl:if test="lsr:Type">
            <b>
                <xsl:value-of select="lsr:Type/@DN"/>:</b>
        </xsl:if>
        <xsl:if test="lsr:StreetAdr">
            <xsl:value-of select="lsr:StreetAdr"/>
            <br/>
        </xsl:if>
        <xsl:if test="lsr:PostalCode">
            <b>Poststed:</b>
            <xsl:value-of select="lsr:PostalCode"/>&#xA0;</xsl:if>
        <xsl:if test="lsr:City">
            <xsl:value-of select="lsr:City"/>
            <br/>
        </xsl:if>
        <xsl:if test="lsr:County">
            <xsl:value-of select="lsr:County/@DN"/>
            <br/>
        </xsl:if>
        <xsl:if test="lsr:Country">
            <xsl:value-of select="lsr:Country/@DN"/>
            <br/>
        </xsl:if>
        <br/>
    </xsl:template>

    <xsl:template match="lsr:ServProvider">
        <!-- utelater rolle <Role>, da denne kun er "Tjenesteyter" -->
        <xsl:apply-templates select="lsr:HCP"/>
    </xsl:template>


    <xsl:template match="lsr:RelServProv">
        <xsl:value-of select="lsr:Relation/@DN"/>:&#160;<xsl:apply-templates select="lsr:HCP"/>
    </xsl:template>


    <xsl:template match="lsr:CopyDest">
        <!-- utelatt rolle-->
        <xsl:apply-templates select="lsr:CopyDestRole"/>
        <xsl:apply-templates select="lsr:HCP"/>
    </xsl:template>
    <xsl:template match="lsr:CopyDestRole">
        <xsl:value-of select="lsr:MsgType/@DN"/>&#160;pr.&#160;<xsl:value-of select="lsr:ActComMethod/@DN"/>&#160;til<br/>
    </xsl:template>

    <xsl:template match="lsr:HCP">
        <xsl:apply-templates select="lsr:Inst"/>
        <xsl:apply-templates select="lsr:HCProf"/>
        <xsl:apply-templates select="MedSpeciality"/>
        <xsl:apply-templates select="lsr:Adress"/>
    </xsl:template>

    <xsl:template match="lsr:Inst">
        <xsl:value-of select="lsr:Name"/>
        <br/>
        <xsl:apply-templates select="Id"/>
        <br/>
        <xsl:apply-templates select="TypeId"/>
        <br/>
        <xsl:for-each select="lsr:Dept">Avdeling: <xsl:value-of select="lsr:Name"/>&#160;Avdelingsnr(lokal): <xsl:value-of select="lsr:Id"/>
            <br/>
        </xsl:for-each>
        <xsl:apply-templates select="lsr:HCPerson"/>
        <xsl:apply-templates select="lsr:AdditionalId"/>
    </xsl:template>

    <xsl:template match="lsr:HCPerson">
        <xsl:value-of select="lsr:Name"/>
        <xsl:if test="lsr:Id">&#160;-&#160;<xsl:value-of select="lsr:TypeId/@V"/>:<xsl:value-of select="lsr:Id"/>
        </xsl:if>
        <br/>
        <xsl:if test="lsr:AdditionalId">
            <xsl:value-of select="lsr:AdditionalId/lsr:Type/@V"/>:<xsl:value-of select="lsr:AdditionalId/lsr:Id"/>
        </xsl:if>
        <br/>
    </xsl:template>

    <xsl:template match="lsr:HCProf">
        <xsl:if test="lsr:Type">
            <xsl:value-of select="lsr:Type/@DN"/>&#160;</xsl:if>
        <xsl:value-of select="lsr:Name"/>
        <xsl:if test="lsr:Id">&#160;-&#160;<xsl:value-of select="lsr:TypeId/@V"/>:<xsl:value-of select="lsr:Id"/>
        </xsl:if>
        <br/>
    </xsl:template>
    <!--Slutt på meldingshodet -->


</xsl:stylesheet>