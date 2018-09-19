<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0"
                xmlns="http://schemas.microsoft.com/wix/2006/wi"
                xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                xmlns:msxsl="urn:schemas-microsoft-com:xslt"
                xmlns:g="http://schemas.microsoft.com/Gadgeteer/2011/Hardware"
                xmlns:clr="http://schemas.microsoft.com/Gadgeteer/2013/BuilderTemplates"
                exclude-result-prefixes="msxsl g clr">
  
  <xsl:param name="SourceName" select="'GadgeteerHardware'" />
  <xsl:param name="IncludePaths" select="''" />

  <xsl:variable name="ModulesDirectoryName" select="'Modules'" />
  <xsl:variable name="MainboardsDirectoryName" select="'Mainboards'" />

  <xsl:output method="xml" indent="yes" />

  <xsl:template name="includes">
    <xsl:param name="paths" />

    <xsl:choose>
      <xsl:when test="contains($paths, ';')">
        <xsl:processing-instruction name="include">
          <xsl:value-of select="substring-before($paths, ';')" />
        </xsl:processing-instruction>
        <xsl:call-template name="includes">
          <xsl:with-param name="path" select="substring-after($paths, ';')" />
        </xsl:call-template>
      </xsl:when>
      <xsl:otherwise>
        <xsl:processing-instruction name="include">
          <xsl:value-of select="$paths" />
        </xsl:processing-instruction>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template match="/g:GadgeteerDefinitions">
    <Wix>
      <xsl:if test="$IncludePaths != ''">
        <xsl:call-template name="includes">
          <xsl:with-param name="paths" select="$IncludePaths" />
        </xsl:call-template>
      </xsl:if>

      <Module Id="$(var.ManufacturerSafeName).$(var.ProjectSafeName)" Language="0" Version="$(var.ProjectVersion)">

        <Package Id="$(var.ProjectPackageID)" Manufacturer="$(var.FullManufacturer)" InstallerVersion="100" Languages="1033" SummaryCodepage="1252" />

        <Property Id="ALLUSERS" Value="2" />

        <Directory Id="TARGETDIR" Name="SourceDir">
          <Directory Id="ProgramFilesFolder" Name="PFiles">
            <Directory Id="$(var.ManufacturerSafeName)" Name="$(var.ManufacturerName)">
              <Directory Id="$(var.ManufacturerSafeName).Gadgeteer" Name="!(loc.GadgeteerDirectoryName)">

                <!-- This directory structure breaks the installer when both modules and mainboards are present in the project. -->

                <xsl:if test="count(g:ModuleDefinitions) > 0">

                  <Directory Name="!(loc.ModulesDirectoryName)">
                    <xsl:attribute name="Id">
                      <xsl:value-of select="$ModulesDirectoryName" />
                    </xsl:attribute>

                    <Directory Id="INSTALLDIR" Name="$(var.ProjectSafeName)" />
                  </Directory>

                </xsl:if>
                
                <xsl:if test="count(g:MainboardDefinitions) > 0">
                  
                  <Directory Name="!(loc.MainboardsDirectoryName)">
                    <xsl:attribute name="Id">
                      <xsl:value-of select="$MainboardsDirectoryName" />
                    </xsl:attribute>

                    <Directory Id="INSTALLDIR" Name="$(var.ProjectSafeName)" />
                  </Directory>
                
                </xsl:if>

              </Directory>
            </Directory>
          </Directory>
        </Directory>

        <ComponentGroupRef Id="$(var.ManufacturerSafeName).$(var.ProjectSafeName)" />

      </Module>
    </Wix>
  </xsl:template>
</xsl:stylesheet>
