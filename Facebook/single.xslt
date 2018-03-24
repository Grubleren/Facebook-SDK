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
        <meta name="programversion" content="1.18"/>

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

        <xsl:for-each select="albums/album">
          <br></br>
          <h2 class="auto-style1">
            <xsl:variable name="href">
              https://www.facebook.com/media/set/?set=oa.<xsl:value-of select="albumId"/>
            </xsl:variable>
            <a href="{$href}">
              <xsl:value-of select="albumName"/>
            </a>
          </h2>

          <xsl:if test="description != ''">
            <br>
              <b>Description</b>
            </br>
            <br>
              <xsl:value-of select="description"/>
            </br>
            <br></br>
          </xsl:if>

          <xsl:if test="comments != ''">
            <br>
              <b>Comments:</b>
            </br>
            <xsl:for-each select="comments/comment">
              <br>
                <xsl:value-of select="text"/>
              </br>
              <xsl:if test="replies != ''">
                <br>
                  <b>Replies:</b>
                </br>

                <xsl:for-each select="replies/reply">
                  <br>
                    <span style="display:inline-block; width: 30;"></span>
                    <xsl:value-of select="text"/>
                  </br>

                </xsl:for-each>
              </xsl:if>
            </xsl:for-each>
            <br></br>
          </xsl:if>

          <xsl:for-each select="photos/page">
            <xsl:for-each select="data">
              <br></br>
              <xsl:variable name="href1">
                https://www.facebook.com/photo.php?fbid=<xsl:value-of select="id"/>
              </xsl:variable>
              <a href="{$href1}">Feed </a>
              <span style="display:inline-block; width: 5;"></span>
              <xsl:variable name="href">
                <xsl:value-of select="source"/>
              </xsl:variable>
              <a href="{$href}">Picture</a>
              <br></br>
                <b>Heading:</b>
              <br></br>
             
                <xsl:value-of select="name"/>
              <br></br>

              <xsl:if test="tags != ''">
                
                  <b>Tags:</b>
                <xsl:for-each select="tags/data">
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

              <xsl:if test="comments != ''">
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

                  <xsl:if test="attachment != ''">
                    <xsl:variable name="hrefa">
                      <xsl:value-of select="attachment"/>
                    </xsl:variable>
                    <a href="{$hrefa}">Attachment</a>
                    <br></br>
                  </xsl:if>

                  <xsl:if test="likes != ''">
                    <b>
                    Likes:&#160;
                  </b>
                    <xsl:for-each select="likes/name">
                      <xsl:value-of select="."/>&#160;
                    </xsl:for-each>
                    <br></br>
                  </xsl:if>

                  <xsl:if test="replies != ''">
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

                      <xsl:if test="likes != ''">
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

              <xsl:if test="likes != ''">
                <b>Picture likes: </b>
                <xsl:for-each select="likes/name">
                  <xsl:value-of select="."/>&#160;
                </xsl:for-each>
                <br></br>
              </xsl:if>

            </xsl:for-each>
          </xsl:for-each>
        </xsl:for-each>
        <br></br>
        <br>
          <b>
            <xsl:value-of select="version"/>
          </b>
        </br>
      </BODY>
    </HTML>
  </xsl:template>
</xsl:stylesheet>
