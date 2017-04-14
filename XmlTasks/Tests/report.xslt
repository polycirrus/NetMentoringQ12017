<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:msxsl="urn:schemas-microsoft-com:xslt" 
                xmlns:lib="http://library.by/catalog" xmlns:user="urn:user-script" 
                exclude-result-prefixes="msxsl lib user">
    <xsl:output method="html" indent="yes"/>
  
  <msxsl:script implements-prefix="user" language="CSharp">
    <![CDATA[
      public static string Today()
      {
        return System.DateTime.Now.Date.ToString("d"); 
      }]]>
  </msxsl:script>

  <xsl:key name="books-by-genre" match="lib:book" use="lib:genre"/>
  <xsl:template match="lib:catalog">
    <html>
      <body>

        <h3>
          <xsl:value-of select="user:Today()"/>
        </h3>
          
        <xsl:for-each select="lib:book[count(. | key('books-by-genre', lib:genre)[1]) = 1]">
          <xsl:sort select="lib:genre"/>
          <table border="1">
            
            <tr>
              <th>Author</th>
              <th>Title</th>
              <th>Registration date</th>
              <th>Publication date</th>
            </tr>
      
          <xsl:for-each select="key('books-by-genre', lib:genre)">
            <tr>
              <td>
                <xsl:value-of select="lib:author"/>
              </td>
              <td>
                <xsl:value-of select="lib:title"/>
              </td>
              <td>
                <xsl:value-of select="lib:registration_date"/>
              </td>
              <td>
                <xsl:value-of select="lib:publish_date"/>
              </td>
            </tr>
          </xsl:for-each>
          
          </table>
          
          <div>
            Total of <xsl:value-of select="lib:genre"/>: <xsl:value-of select="count(key('books-by-genre', lib:genre))"/>
          </div>
        </xsl:for-each>

        <div>
          Grand total: <xsl:value-of select="count(lib:book)"/>
        </div>
    
      </body>
    </html>
  </xsl:template>
  
  <!--<xsl:template match="lib:catalog">
    <html lang="en">
      <body>
        <h3>
          <xsl:value-of select="user:Today()"/>
        </h3>

        <xsl:for-each select="lib:book">
          <xsl:sort select="lib:genre"/>

          <div>Previous = <xsl:value-of select="preceding-sibling::lib:book[1]/lib:genre"/></div>
          <div>Current = <xsl:value-of select="lib:genre"/></div>
          <xsl:if test="preceding-sibling::lib:book[1]/lib:genre != lib:genre">
            <xsl:if test="count(preceding-sibling::lib:book) > 0">
              <xsl:text disable-output-escaping="yes"><![CDATA[</table>]]></xsl:text>
              <div>
                Total: <xsl:value-of select="count(//lib:book[lib:genre = preceding-sibling::lib:book[1]/lib:genre])"/>
              </div>
            </xsl:if>

            <xsl:text disable-output-escaping="yes"><![CDATA[<table>]]></xsl:text>
              <tr>
                <th>Author</th>
                <th>Title</th>
                <th>Registration date</th>
                <th>Publication date</th>
              </tr>
            </xsl:if>

            <tr>
              <td>
                <xsl:value-of select="lib:author"/>
              </td>
              <td>
                <xsl:value-of select="lib:title"/>
              </td>
              <td>
                <xsl:value-of select="lib:registration_date"/>
              </td>
              <td>
                <xsl:value-of select="lib:publish_date"/>
              </td>
            </tr>

        </xsl:for-each>
        <div>
          Library total: <xsl:value-of select="count(//lib:book)"/>
        </div>
      </body>
    </html>
  </xsl:template>-->

</xsl:stylesheet>
