<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0">
  <xsl:template match="root">
    <HTML>
      <head>
        <meta http-equiv="Content-Type" content="text/html; charset=windows-1252"/>
        <title>
          <xsl:value-of select="groupName"/>
        </title>
        <meta name="author" content="Jens Hee"/>
        <meta name="program" content="BlackFrog"/>
        <meta name="programversion" content="1.27"/>

      </head>
      <BODY  bgcolor="CFDAC4" topmargin="0" leftmargin="40" lang="da-DK" dir="ltr">
        <h1 class="auto-style1">
          <xsl:variable name="href">
            https://www.facebook.com/groups/<xsl:value-of select="groupId"/>
          </xsl:variable>
          <a href="{$href}">
            <xsl:value-of select="groupName"/>
          </a>
        </h1>

        <xsl:if test="groupDescription != ''">
          <br>
            <b>Group description</b>
          </br>
          <br>
            <xsl:value-of select="groupDescription"/>
          </br>
          <br></br>
          <br></br>
        </xsl:if>

        <xsl:for-each select="albums/photos/page">
          <xsl:for-each select="data">

            <xsl:if test="link">
              <xsl:variable name="hrefl">
                <xsl:value-of select="link"/>
              </xsl:variable>
              <a href="{$hrefl}">
                <xsl:value-of select="link"/>
              </a>
              <br></br>
            </xsl:if>

            <xsl:if test="full_picture">
              <xsl:variable name="hrefp">
                <xsl:value-of select="full_picture"/>
              </xsl:variable>
              <a href="{$hrefp}">Picture</a>
              <br></br>
            </xsl:if>

            <xsl:if test="story">
              <b>Story:</b>
              <br></br>
              <xsl:if test="from/name">
                <xsl:value-of select="from/name"/>:&#160;
              </xsl:if>
              <xsl:value-of select="story"/>
              <br></br>
            </xsl:if>

            <xsl:if test="message">
              <b>Message:</b>
              <br></br>
              <xsl:if test="from/name">
                <xsl:value-of select="from/name"/>:&#160;
              </xsl:if>
              <xsl:value-of select="message"/>
              <br></br>
            </xsl:if>

            <xsl:if test="created_time">
              <b>Created time:&#160;</b>
              <xsl:value-of select="created_time"/>
              <br></br>
            </xsl:if>

            <xsl:if test="tags/page/tags != ''">

              <b>Tags:</b>
              <xsl:for-each select="tags/page/tags/data">
                <xsl:variable name="x">
                  <xsl:value-of select="x"/>
                </xsl:variable>
                <xsl:variable name="y">
                  <xsl:value-of select="y"/>
                </xsl:variable>
                <br>
                  <xsl:value-of select="name"/>: (<xsl:value-of select='format-number(number($x),"#.000")'/>, <xsl:value-of select='format-number(number($y),"#.000")'/>)
                </br>

              </xsl:for-each>
              <br></br>
            </xsl:if>

            <xsl:if test="comments">
              <b>Comments:</b>
              <br></br>
              <xsl:for-each select="comments/comment">

                <xsl:variable name="ccount">
                  <xsl:value-of select="position()"/>
                </xsl:variable>

                <b>
                  C<xsl:value-of select='string($ccount)'/>:&#160;
                </b>
                <xsl:value-of select="text"/>
                <br></br>

                <xsl:if test="attachment">
                  <xsl:variable name="hrefa">
                    <xsl:value-of select="attachment"/>
                  </xsl:variable>
                  <a href="{$hrefa}">Attachment</a>
                  <br></br>
                </xsl:if>

                <xsl:if test="created_time">
                  <b>Created time:&#160;</b>
                  <xsl:value-of select="created_time"/>
                  <br></br>
                </xsl:if>

                <xsl:if test="likes">
                  <b>
                    Likes:&#160;
                  </b>
                  <xsl:for-each select="likes/name">
                    <xsl:value-of select="."/>&#160;
                  </xsl:for-each>
                  <br></br>
                </xsl:if>

                <xsl:if test="replies">
                  <b>Replies:</b>
                  <br></br>
                  <xsl:for-each select="replies/reply">

                    <xsl:variable name="rcount">
                      <xsl:value-of select="position()"/>
                    </xsl:variable>

                    <b>
                      &#160;&#160;&#160;&#160;&#160;
                      R<xsl:value-of select='string($rcount)'/>:&#160;
                    </b>
                    <xsl:value-of select="text"/>
                    <br></br>

                    <xsl:if test="created_time">
                      <b>
                        &#160;&#160;&#160;&#160;&#160;
                        Created time:&#160;
                      </b>
                      <xsl:value-of select="created_time"/>
                      <br></br>
                    </xsl:if>

                    <xsl:if test="likes">
                      <b>
                        &#160;&#160;&#160;&#160;&#160;
                        Likes:&#160;
                      </b>
                      <xsl:for-each select="likes/name">
                        <xsl:value-of select="."/>&#160;
                      </xsl:for-each>
                      <br></br>
                    </xsl:if>
                  </xsl:for-each>
                </xsl:if>

              </xsl:for-each>
            </xsl:if>

            <xsl:if test="likes">
              <b>Picture likes: </b>
              <xsl:for-each select="likes/name">
                <xsl:value-of select="."/>&#160;
              </xsl:for-each>
              <br></br>
            </xsl:if>
            <br></br>


          </xsl:for-each>
        </xsl:for-each>
        <br>
          <b>
            <xsl:value-of select="version"/>
          </b>
        </br>
      </BODY>
    </HTML>
  </xsl:template>
</xsl:stylesheet>
