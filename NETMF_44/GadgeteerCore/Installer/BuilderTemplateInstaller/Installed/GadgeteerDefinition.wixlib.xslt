<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0"
                xmlns="http://schemas.microsoft.com/wix/2006/wi"
                xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                xmlns:msxsl="urn:schemas-microsoft-com:xslt"
                xmlns:g="http://schemas.microsoft.com/Gadgeteer/2011/Hardware"
                xmlns:clr="http://schemas.microsoft.com/Gadgeteer/2013/BuilderTemplates"
                exclude-result-prefixes="msxsl g clr">
  
  <xsl:param name="SourceName" select="'GadgeteerHardware'" />

  <xsl:variable name="ModulesId" select="'M'" />
  <xsl:variable name="ModulesDirectoryName" select="'Modules'" />
  
  <xsl:variable name="MainboardsId" select="'B'" />
  <xsl:variable name="MainboardsDirectoryName" select="'Mainboards'" />

  <xsl:output method="xml" indent="yes" />
  
  <msxsl:script language="CSharp" implements-prefix="clr">
    <![CDATA[
    class DistinctNodeIterator : XPathNodeIterator
    {
      private Hashtable _used = new Hashtable();
      private XPathNodeIterator _navigator;
      private int _position;
      
      public DistinctNodeIterator(XPathNodeIterator navigator) { _navigator = navigator; }
    
      public override XPathNavigator Current { get { return _navigator.Current; } }
      public override int CurrentPosition { get { return _position; } }
      public override XPathNodeIterator Clone() { return new DistinctNodeIterator(_navigator.Clone()); }
     
      public override bool MoveNext()
      {
        while (_navigator.MoveNext())
        {
          string value = _navigator.Current.Value;
          if (!_used.ContainsKey(value))
          {
            _used.Add(value, _navigator.Current);
            ++_position;
            return true;
          }
        }
        
        return false;
      }
    }
    
    public string NewGuid() 
    {
      return System.Guid.NewGuid().ToString(); 
    }
    public XPathNodeIterator Distinct(XPathNodeIterator nodes)
    {
      return new DistinctNodeIterator(nodes);
    }
    ]]>
  </msxsl:script>

  <xsl:template name="file">
    <xsl:param name="path" />
    <xsl:param name="sourcePath" />
    <xsl:param name="prefix" select="''" />
    <xsl:param name="postfix" select="''" />

    <xsl:choose>
      <xsl:when test="contains($path, '\')">
        <Directory>
          <xsl:attribute name="Id">
            <xsl:value-of select="concat($prefix, substring-before($path, '\'))"/>
          </xsl:attribute>
          <xsl:attribute name="Name">
            <xsl:value-of select="substring-before($path, '\')" />
          </xsl:attribute>
          <xsl:call-template name="file">
            <xsl:with-param name="path" select="substring-after($path, '\')" />
            <xsl:with-param name="sourcePath" select="$sourcePath" />
            <xsl:with-param name="prefix" select="$prefix" />
            <xsl:with-param name="postfix" select="concat($postfix, '.', substring-before($path, '\'))" />
          </xsl:call-template>
        </Directory>
      </xsl:when>
      <xsl:otherwise>
        <Component>
          <xsl:attribute name="Id">
            <xsl:value-of select="concat($prefix, $path, $postfix)" />
          </xsl:attribute>
          <xsl:attribute name="Guid">
            <xsl:value-of select="clr:NewGuid()" />
          </xsl:attribute>
          <File KeyPath="yes">
            <xsl:attribute name="Id">
              <xsl:value-of select="concat($prefix, $path, $postfix)" />
            </xsl:attribute>
            <xsl:attribute name="Source">
              <xsl:value-of select="concat($sourcePath, $path)" />
            </xsl:attribute>
          </File>
        </Component>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="fileref">
    <xsl:param name="path" />
    <xsl:param name="sourcePath" />
    <xsl:param name="prefix" select="''" />
    <xsl:param name="postfix" select="''" />

    <xsl:choose>
      <xsl:when test="contains($path, '\')">
        <xsl:call-template name="fileref">
          <xsl:with-param name="path" select="substring-after($path, '\')" />
          <xsl:with-param name="sourcePath" select="$sourcePath" />
          <xsl:with-param name="prefix" select="$prefix" />
          <xsl:with-param name="postfix" select="concat($postfix, '.', substring-before($path, '\'))" />
        </xsl:call-template>
      </xsl:when>
      <xsl:otherwise>
        <ComponentRef>
          <xsl:attribute name="Id">
            <xsl:value-of select="concat($prefix, $path, $postfix)" />
          </xsl:attribute>
        </ComponentRef>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template match="/g:GadgeteerDefinitions">

    <xsl:comment xml:space="preserve"> ╔════════════════════════════════════════════════════════════════════════════════════════════╗ </xsl:comment>
    <xsl:comment xml:space="preserve"> ║ WARNING - This is a generated file. Any changes will be lost when the file is regenerated. ║ </xsl:comment>
    <xsl:comment xml:space="preserve"> ╚════════════════════════════════════════════════════════════════════════════════════════════╝ </xsl:comment>

    <Wix>
      <xsl:call-template name="registry" />
      
      <xsl:call-template name="xml">
        <xsl:with-param name="id" select="$ModulesId" />
        <xsl:with-param name="dir" select="$ModulesDirectoryName" />
      </xsl:call-template>
      <xsl:call-template name="xml">
        <xsl:with-param name="id" select="$MainboardsId" />
        <xsl:with-param name="dir" select="$MainboardsDirectoryName" />
      </xsl:call-template>

      <xsl:apply-templates select="g:ModuleDefinitions/g:ModuleDefinition" mode="fragments" />
      <xsl:apply-templates select="g:MainboardDefinitions/g:MainboardDefinition" mode="fragments" />

      <Fragment>
        <ComponentGroup Id="$(var.ManufacturerSafeName).$(var.ProjectSafeName)">
          <ComponentRef Id="VS.AddIn" />
          <xsl:apply-templates select="g:ModuleDefinitions/g:ModuleDefinition" mode="references" />
          <xsl:apply-templates select="g:MainboardDefinitions/g:MainboardDefinition" mode="references" />
        </ComponentGroup>
      </Fragment>
    </Wix>
  </xsl:template>

  <xsl:template match="g:ModuleDefinition" mode="fragments">
    <xsl:call-template name="definition">
      <xsl:with-param name="id" select="$ModulesId" />
      <xsl:with-param name="dir" select="$ModulesDirectoryName" />
    </xsl:call-template>
  </xsl:template>  
  <xsl:template match="g:MainboardDefinition" mode="fragments">
    <xsl:call-template name="definition">
      <xsl:with-param name="id" select="$MainboardsId" />
      <xsl:with-param name="dir" select="$MainboardsDirectoryName" />
    </xsl:call-template>
  </xsl:template>
  
  <xsl:template match="g:ModuleDefinition" mode="references">
    <xsl:call-template name="definitionref">
      <xsl:with-param name="id" select="$ModulesId" />
    </xsl:call-template>
  </xsl:template>
  <xsl:template match="g:MainboardDefinition" mode="references">
    <xsl:call-template name="definitionref">
      <xsl:with-param name="id" select="$MainboardsId" />
    </xsl:call-template>
  </xsl:template>

  <xsl:template name="xml">
    <xsl:param name="id" />
    <xsl:param name="dir" />

    <Fragment>
      <DirectoryRef Id="INSTALLDIR">

        <!-- GadgeteerDefinitions for mainboards -->
        <Component>
          <xsl:attribute name="Id">
            <xsl:value-of select="concat($id, '.', $SourceName)"/>
          </xsl:attribute>
          <xsl:attribute name="Guid">
            <xsl:value-of select="clr:NewGuid()" />
          </xsl:attribute>

          <File KeyPath="yes">
            <xsl:attribute name="Id">
              <xsl:value-of select="concat($id, '.', $SourceName, '.xml')"/>
            </xsl:attribute>
            <xsl:attribute name="Source">
              <xsl:value-of select="concat('$(var.ProjectDir)\', $SourceName, '.xml')" />
            </xsl:attribute>
          </File>
        </Component>

      </DirectoryRef>
    </Fragment>    
  </xsl:template>
  <xsl:template name="definition">
    <xsl:param name="id" />
    <xsl:param name="dir" />
    <xsl:variable name="DefinitionId" select="concat($id, position())" />

    <!-- Image -->
    <xsl:if test="@Image">
      <Fragment>
        <DirectoryRef Id="INSTALLDIR">
          <xsl:call-template name="file">
            <xsl:with-param name="path" select="@Image" />
            <xsl:with-param name="sourcePath" select="'$(var.ProjectDir)'" />
            <xsl:with-param name="prefix" select="concat($DefinitionId, '.')" />
          </xsl:call-template>
        </DirectoryRef>
      </Fragment>
    </xsl:if>

    <xsl:for-each select="g:Assemblies/g:Assembly[starts-with(@Name, 'Gadgeteer')=false]">
      <xsl:variable name="AssemblyId" select="concat('A', position())" />
      
      <Fragment>
        <DirectoryRef Id="INSTALLDIR">

          <!-- Assemblies -->
          <Directory>
            <xsl:attribute name="Name">
              <xsl:value-of select="concat('NETMF ', @MFVersion)" />
            </xsl:attribute>
            <xsl:attribute name="Id">
              <xsl:value-of select="concat($DefinitionId, '.', $AssemblyId, '.', @MFVersion)" />
            </xsl:attribute>
            <xsl:attribute name="FileSource">
              <xsl:value-of select="concat('$(var.ProjectDir)..\$(var.ProjectSafeName)_', substring-before(@MFVersion, '.'), substring-after(@MFVersion, '.'), '\bin\Release\')" />
            </xsl:attribute>

            <!-- MSIL Assemblies -->
            <Component>
              <xsl:attribute name="Id">
                <xsl:value-of select="concat($DefinitionId, '.', $AssemblyId, '.',  @MFVersion)" />
              </xsl:attribute>
              <xsl:attribute name="Guid">
                <xsl:value-of select="clr:NewGuid()" />
              </xsl:attribute>

              <File KeyPath="yes">
                <xsl:attribute name="Id">
                  <xsl:value-of select="concat($DefinitionId, '.', $AssemblyId, '.', @MFVersion, '.dll')" />
                </xsl:attribute>
                <xsl:attribute name="Name">
                  <xsl:value-of select="concat(@Name, '.dll')" />
                </xsl:attribute>
              </File>
              <File>
                <xsl:attribute name="Id">
                  <xsl:value-of select="concat($DefinitionId, '.', $AssemblyId, '.', @MFVersion, '.pdb')" />
                </xsl:attribute>
                <xsl:attribute name="Name">
                  <xsl:value-of select="concat(@Name, '.pdb')" />
                </xsl:attribute>
              </File>
              <File>
                <xsl:attribute name="Id">
                  <xsl:value-of select="concat($DefinitionId, '.', $AssemblyId, '.', @MFVersion, '.xml')" />
                </xsl:attribute>
                <xsl:attribute name="Name">
                  <xsl:value-of select="concat(@Name, '.xml')" />
                </xsl:attribute>
              </File>
            </Component>

            <!-- BE Assemblies -->
            <Directory Name="be">
              <xsl:attribute name="Id">
                <xsl:value-of select="concat($DefinitionId, '.', $AssemblyId, '.', @MFVersion, '.be')" />
              </xsl:attribute>

              <Component>
                <xsl:attribute name="Id">
                  <xsl:value-of select="concat($DefinitionId, '.', $AssemblyId, '.', @MFVersion, '.be')" />
                </xsl:attribute>
                <xsl:attribute name="Guid">
                  <xsl:value-of select="clr:NewGuid()" />
                </xsl:attribute>

                <File KeyPath="yes">
                  <xsl:attribute name="Id">
                    <xsl:value-of select="concat($DefinitionId, '.', $AssemblyId, '.', @MFVersion, '.be.dll')" />
                  </xsl:attribute>
                  <xsl:attribute name="Name">
                    <xsl:value-of select="concat(@Name, '.dll')" />
                  </xsl:attribute>
                </File>
                <File>
                  <xsl:attribute name="Id">
                    <xsl:value-of select="concat($DefinitionId, '.', $AssemblyId, '.', @MFVersion, '.be.pdb')" />
                  </xsl:attribute>
                  <xsl:attribute name="Name">
                    <xsl:value-of select="concat(@Name, '.pdb')" />
                  </xsl:attribute>
                </File>
                <File>
                  <xsl:attribute name="Id">
                    <xsl:value-of select="concat($DefinitionId, '.', $AssemblyId, '.', @MFVersion, '.be.pdbx')" />
                  </xsl:attribute>
                  <xsl:attribute name="Name">
                    <xsl:value-of select="concat(@Name, '.pdbx')" />
                  </xsl:attribute>
                </File>
                <File>
                  <xsl:attribute name="Id">
                    <xsl:value-of select="concat($DefinitionId, '.', $AssemblyId, '.', @MFVersion, '.be.pe')" />
                  </xsl:attribute>
                  <xsl:attribute name="Name">
                    <xsl:value-of select="concat(@Name, '.pe')" />
                  </xsl:attribute>
                </File>
              </Component>
            </Directory>

            <!-- LE Assemblies -->
            <Directory Name="le">
              <xsl:attribute name="Id">
                <xsl:value-of select="concat($DefinitionId, '.', $AssemblyId, '.', @MFVersion, '.le')" />
              </xsl:attribute>

              <Component>
                <xsl:attribute name="Id">
                  <xsl:value-of select="concat($DefinitionId, '.', $AssemblyId, '.', @MFVersion, '.le')" />
                </xsl:attribute>
                <xsl:attribute name="Guid">
                  <xsl:value-of select="clr:NewGuid()" />
                </xsl:attribute>

                <File KeyPath="yes">
                  <xsl:attribute name="Id">
                    <xsl:value-of select="concat($DefinitionId, '.', $AssemblyId, '.', @MFVersion, '.le.dll')" />
                  </xsl:attribute>
                  <xsl:attribute name="Name">
                    <xsl:value-of select="concat(@Name, '.dll')" />
                  </xsl:attribute>
                </File>
                <File>
                  <xsl:attribute name="Id">
                    <xsl:value-of select="concat($DefinitionId, '.', $AssemblyId, '.', @MFVersion, '.le.pdb')" />
                  </xsl:attribute>
                  <xsl:attribute name="Name">
                    <xsl:value-of select="concat(@Name, '.pdb')" />
                  </xsl:attribute>
                </File>
                <File>
                  <xsl:attribute name="Id">
                    <xsl:value-of select="concat($DefinitionId, '.', $AssemblyId, '.', @MFVersion, '.le.pdbx')" />
                  </xsl:attribute>
                  <xsl:attribute name="Name">
                    <xsl:value-of select="concat(@Name, '.pdbx')" />
                  </xsl:attribute>
                </File>
                <File>
                  <xsl:attribute name="Id">
                    <xsl:value-of select="concat($DefinitionId, '.', $AssemblyId, '.', @MFVersion, '.le.pe')" />
                  </xsl:attribute>
                  <xsl:attribute name="Name">
                    <xsl:value-of select="concat(@Name, '.pe')" />
                  </xsl:attribute>
                </File>
              </Component>
            </Directory>

          </Directory>

        </DirectoryRef>
      </Fragment>
    </xsl:for-each>
  </xsl:template>
  <xsl:template name="definitionref">
    <xsl:param name="id" />
    <xsl:variable name="DefinitionId" select="concat($id, position())" />
    <xsl:variable name="mfSupported" select="clr:Distinct(g:Assemblies/g:Assembly/@MFVersion)" />
    
    <xsl:for-each select="$mfSupported">
      <xsl:variable name="MFVersion" select="current()" />
      <ComponentRef >
        <xsl:attribute name="Id">
          <xsl:value-of select="concat('Registry.GAC.', $MFVersion)" />
        </xsl:attribute>
      </ComponentRef>
    </xsl:for-each>

    <ComponentRef>
      <xsl:attribute name="Id">
        <xsl:value-of select="concat($id, '.', $SourceName)"/>
      </xsl:attribute>
    </ComponentRef>
    
    <!-- Image -->
    <xsl:if test="@Image">
      <xsl:call-template name="fileref">
        <xsl:with-param name="path" select="@Image" />
        <xsl:with-param name="sourcePath" select="'$(var.ProjectDir)'" />
        <xsl:with-param name="prefix" select="concat($DefinitionId, '.')" />
      </xsl:call-template>
    </xsl:if>

    <xsl:for-each select="g:Assemblies/g:Assembly[starts-with(@Name, 'Gadgeteer')=false]">
      <xsl:variable name="AssemblyId" select="concat('A', position())" />
        
      <ComponentRef>
        <xsl:attribute name="Id">
          <xsl:value-of select="concat($DefinitionId, '.', $AssemblyId, '.', @MFVersion)" />
        </xsl:attribute>
      </ComponentRef>
      <ComponentRef>
        <xsl:attribute name="Id">
          <xsl:value-of select="concat($DefinitionId, '.', $AssemblyId, '.', @MFVersion, '.be')" />
        </xsl:attribute>
      </ComponentRef>
      <ComponentRef>
        <xsl:attribute name="Id">
          <xsl:value-of select="concat($DefinitionId, '.', $AssemblyId, '.', @MFVersion, '.le')" />
        </xsl:attribute>
      </ComponentRef>
    </xsl:for-each>
  </xsl:template>

  <xsl:template name="registry">
    <xsl:variable name="mfSupportedAll" select="clr:Distinct(//g:Assembly/@MFVersion)" />
    
    <Fragment>
      <DirectoryRef Id="TARGETDIR">
        <!-- This is marked as Win32 so that it puts it in the correct location in the registry on x64 machines with registry redirection. -->
        <Component Id="VS.AddIn" Win64="no">
          <xsl:attribute name="Guid">
            <xsl:value-of select="clr:NewGuid()" />
          </xsl:attribute>

          <!-- Registry key for the designer - specifies the folder containing GadgeteerHardware.xml -->
          <RegistryValue Id="VS.AddIn" KeyPath="yes" Type="string" Root="HKLM" Key="SOFTWARE\Microsoft\.NETGadgeteer\v2\HardwareDefinitionFolders\$(var.AssemblyName)" Value="[INSTALLDIR]" />
        </Component>
      </DirectoryRef>
    </Fragment>

    <xsl:for-each select="$mfSupportedAll">
      <xsl:variable name="MFVersion" select="current()" />
      <Fragment>
        <DirectoryRef Id="TARGETDIR">
          <Component Win64="no">
            <xsl:attribute name="Id">
              <xsl:value-of select="concat('Registry.GAC.', $MFVersion)" />
            </xsl:attribute>
            <xsl:attribute name="Guid">
              <xsl:value-of select="clr:NewGuid()" />
            </xsl:attribute>
            <RegistryValue Root="HKLM" Type="string">
              <xsl:attribute name="Id">
                <xsl:value-of select="concat('Registry.GAC.', $MFVersion)" />  
              </xsl:attribute>
              <xsl:attribute name="Key">
                <xsl:value-of select="concat('SOFTWARE\Microsoft\.NETMicroFramework\v', $MFVersion, '\AssemblyFoldersEx\$(var.AssemblyName)')" />
              </xsl:attribute>
              <xsl:attribute name="Value">
                <xsl:value-of select="concat('[INSTALLDIR]NETMF ', $MFVersion)" />
              </xsl:attribute>
            </RegistryValue>
          </Component>
        </DirectoryRef>
      </Fragment>
    </xsl:for-each>
  </xsl:template>
</xsl:stylesheet>
