<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:msxsl="urn:schemas-microsoft-com:xslt" xmlns:lib="http://library.by/catalog" exclude-result-prefixes="msxsl"
>
    <xsl:output method="xml" indent="yes"/>

    <xsl:template match="@* | node()">
        <xsl:copy>
            <xsl:apply-templates select="@* | node()"/>
        </xsl:copy>
    </xsl:template>

  <xsl:template match="lib:catalog">
    <rss version="2.0">
      <channel>
        <xsl:apply-templates />
      </channel>
    </rss>
  </xsl:template>

  <xsl:template match="lib:catalog/lib:book">
    <item>
      <xsl:apply-templates select="lib:author | lib:genre | lib:title | lib:description | lib:registration_date"/>
    </item>
  </xsl:template>

  <xsl:template match="lib:catalog/lib:book/lib:author">
    <author>
      <xsl:value-of select="."/>
    </author>
  </xsl:template>
  
  <xsl:template match="lib:catalog/lib:book/lib:genre">
    <category><xsl:value-of select="."/></category>
  </xsl:template>
  
  <xsl:template match="lib:catalog/lib:book/lib:title">
    <title><xsl:value-of select="."/></title>
  </xsl:template>
  
  <xsl:template match="lib:catalog/lib:book/lib:description">
    <description><xsl:value-of select="."/></description>
  </xsl:template>

  <xsl:template match="lib:catalog/lib:book/lib:registration_date">
    <pubDate><xsl:value-of select="."/></pubDate>
  </xsl:template>
  
  <xsl:template match="lib:catalog/lib:book/lib:isbn[parent::lib:book/lib:genre = 'Computer']">
    <link>http://my.safaribooksonline.com/<xsl:value-of select="."/>/</link>
  </xsl:template>
</xsl:stylesheet>
