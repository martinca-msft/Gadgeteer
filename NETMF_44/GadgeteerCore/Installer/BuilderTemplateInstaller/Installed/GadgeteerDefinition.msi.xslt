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

      <xsl:if test="count(g:ModuleDefinitions) > 0">
        <xsl:call-template name="product">
          <xsl:with-param name="type" select="'Modules'" />
        </xsl:call-template>
      </xsl:if>

      <xsl:if test="count(g:MainboardDefinitions) > 0">
        <xsl:call-template name="product">
          <xsl:with-param name="type" select="'Mainboards'" />
        </xsl:call-template>
      </xsl:if>

    </Wix>
  </xsl:template>
  
  <xsl:template name="product">
    <xsl:param name="type" select="''" /> <!-- 'Modules' or 'Mainboards' -->
    
      <Product Id="*" Manufacturer="$(var.ManufacturerName)" Version="$(var.ProjectVersion)" UpgradeCode="$(var.ProjectUpgradeCode)" Language="1033" Codepage="1252">
        <xsl:attribute name="Name">
          <xsl:value-of select="concat('$(var.ProjectSafeName) !(loc.', $type, 'ForGadgeteer)')" />
        </xsl:attribute>

        <Package Id="*" Manufacturer="$(var.ManufacturerName)" InstallPrivileges="elevated" InstallerVersion="100" Languages="1033" SummaryCodepage="1252" Compressed="yes" />

        <Property Id="ISPREVIOUSVERSIONINSTALLED" Secure="yes" />
        <Property Id="ISSAMEVERSIONINSTALLED" Secure="yes"/>
        <Upgrade Id="$(var.ProjectUpgradeCode)">
          <UpgradeVersion Property="ISPREVIOUSVERSIONINSTALLED" IncludeMaximum="no"  Maximum="$(var.ProjectVersion)"
                                                                IncludeMinimum="yes"                                 OnlyDetect="no"  />
          
          <UpgradeVersion Property="ISNEWERVERSIONINSTALLED"    IncludeMinimum="no"  Minimum="$(var.ProjectVersion)" OnlyDetect="yes" />
          
          <UpgradeVersion Property="ISSAMEVERSIONINSTALLED"     IncludeMinimum="yes" Minimum="$(var.ProjectVersion)"
                                                                IncludeMaximum="yes" Maximum="$(var.ProjectVersion)" OnlyDetect="yes" />
        </Upgrade>

        <CustomAction Id="NoDowngrade" Error="!(loc.NewerVersionFoundPrefix) $(var.ProjectSafeName) !(loc.NewerVersionFoundSuffix)" />
        <CustomAction Id="SameVersionError" Error="!(loc.SameVersionInstalledPrefix) $(var.ProjectSafeName) !(loc.SameVersionInstalledSuffix)" />
        <CustomAction Id="GadgeteerCoreNotInstalledError" Error="!(loc.GadgeteerCoreIsNotInstalled)"/>

        <Property Id="ISGADGETEERCOREINSTALLED">
          <ComponentSearch Id="GadgeteerCoreInstalled" Guid="20CDFFF1-2E36-45CE-A4C1-1DFA1FB123CC" Type="file" />
        </Property>

        <!--ARP stuff (Add/Remove Programs)-->
        <Property Id="ARPNOMODIFY" Value="1" />
        <Property Id="ARPNOREPAIR" Value="1" />

        <!-- Files -->
        <Media Id="1" Cabinet="product.cab" EmbedCab="yes" />

        <Directory Id="TARGETDIR" Name="SourceDir">
          <Merge Id="$(var.ManufacturerSafeName).$(var.ProjectSafeName)" Language="1033" SourceFile="$(var.BinPath)\..\Installer\$(var.ProjectSafeName).msm" DiskId="1" />
        </Directory>

        <Feature Id="$(var.ManufacturerSafeName).$(var.ProjectSafeName)" Title="$(var.ProjectName)" Level="1">
          <MergeRef Id="$(var.ManufacturerSafeName).$(var.ProjectSafeName)" />
        </Feature>

        <Icon Id="AddRemoveIcon" SourceFile="$(var.ApplicationIcon)" />
        <Property Id="ARPPRODUCTICON" Value="AddRemoveIcon"/>

        <InstallExecuteSequence>
          <RemoveExistingProducts Before="InstallInitialize"/>

          <Custom Action="NoDowngrade" After="FindRelatedProducts">ISNEWERVERSIONINSTALLED</Custom>
          <Custom Action="SameVersionError" After="FindRelatedProducts">ISSAMEVERSIONINSTALLED</Custom>
          <Custom Action="GadgeteerCoreNotInstalledError" Before="InstallInitialize"><![CDATA[&$(var.ManufacturerSafeName).$(var.ProjectSafeName)>2 AND NOT ISGADGETEERCOREINSTALLED]]></Custom>
        </InstallExecuteSequence>
      </Product>
    
  </xsl:template>
</xsl:stylesheet>
