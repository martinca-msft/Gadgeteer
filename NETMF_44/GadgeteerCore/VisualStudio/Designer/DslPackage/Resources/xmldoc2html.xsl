<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:msxsl="urn:schemas-microsoft-com:xslt" exclude-result-prefixes="msxsl"
>
	<xsl:output method="html" indent="yes"/>

	<xsl:param name="moreInfoUrl"/>

	<xsl:template match="/">
		<head>
			<title>
				<xsl:value-of select="doc/assembly/name"/>
			</title>
			<style>
				div, h1, h2, .pageTitle, table, dl, h3, p {font-family: "Segoe UI", Verdana, Arial; margin: 0px 12px 12px 12px;}
				div, dl, td, li, p {font-size: 13px;}
				.pageTitle {font-size: 32px; color: #3f529c; font-weight:bold;}
				table {margin-top: 0px; border-left: 1px solid #bbb; border-bottom: 1px solid #bbb;}
				th {font-size: 13px; height: 21px; text-align: left; color: #000; padding: 4px; font-weight: normal; background-color: #E5E5E5; border-top: 1px solid #bbb; border-right: 1px solid #bbb;}
				td {vertical-align:top; border-top:1px solid #bbb; border-right:1px solid #bbb; padding: 4px;}
				.typeName {font-weight: bold; font-size: 15px;}
				h1 {font-size: 24px;}
				h2 {font-size: 18px;}
				h3 {font-size: 14px;}
				dt {font-style: italic; }
				.note {border: 1px solid #bbb; background-color: #FCFEC5; padding: 11px; margin-left: 0px;}
				li {font-family: "Segoe UI", Verdana, Arial;}
				div p {margin-left: 0px;}
			</style>
		</head>
		<div class="pageTitle">
			<xsl:value-of select="//assembly/name"/>
		</div>
		<div>
			<b>Assembly:</b>&#160;<xsl:value-of select="//assembly/name"/>.dll
		</div>
		<xsl:if test="$moreInfoUrl != ''">
			<div>
				<b>More information:</b>&#160;<a href="{$moreInfoUrl}">
					<xsl:value-of select="$moreInfoUrl"/>
				</a>
			</div>
		</xsl:if>
		<xsl:apply-templates select="//member[starts-with(@name,'T:')]" mode="type"/>
	</xsl:template>

	<xsl:template match="member" mode="type">
		<xsl:variable name="fName" select="substring-after(@name,'T:')"/>
		<h1>
			Type:
			<a>
				<xsl:attribute name="name">
					<xsl:value-of select="$fName"/>
				</xsl:attribute>
			</a><xsl:value-of select="$fName"/>
		</h1>
		<xsl:apply-templates select="summary"/>
		<xsl:if test="param">
			<h3>Parameters</h3>
			<xsl:apply-templates select="param"/>
		</xsl:if>
		<xsl:apply-templates select="example"/>

		<xsl:variable name="constructorName" select="concat('M:',$fName,'.#ctor')"/>
		<xsl:if test="//member[starts-with(@name, $constructorName)]">
			<h2>Constructors</h2>
			<table cellpadding="0" cellspacing="0">
				<tr>
					<th>Name</th>
					<th>Description</th>
				</tr>
				<xsl:apply-templates select="//member[starts-with(@name, $constructorName)]" mode="constructor"/>
			</table>
		</xsl:if>

		<xsl:variable name="propertyName" select="concat('P:', $fName)"/>
		<xsl:variable name="propertyBody">
			<xsl:apply-templates select="//member[starts-with(@name, $propertyName)]" mode="property">
				<xsl:with-param name="propertyName" select="$propertyName"/>
			</xsl:apply-templates>
		</xsl:variable>
		<!--<xsl:if test="//member[starts-with(@name, $propertyName)]">-->
		<xsl:if test="$propertyBody != ''">
			<h2>Properties</h2>
			<table cellpadding="0" cellspacing="0">
				<tr>
					<th>Name</th>
					<th>Description</th>
				</tr>
				<xsl:copy-of select="$propertyBody"/>
				<!--<xsl:apply-templates select="//member[starts-with(@name, $propertyName)]" mode="property"/>-->
			</table>
		</xsl:if>

		<xsl:variable name="methodName" select="concat('M:',$fName)"/>
		<xsl:if test="//member[starts-with(@name, $methodName) and not(contains(@name, '#ctor'))]">
			<h2>Methods</h2>
			<table cellpadding="0" cellspacing="0">
				<tr>
					<th>Name</th>
					<th>Description</th>
				</tr>
				<!--xsl:apply-templates select="//member[starts-with(@name, 'M:') and contains(@name, $fName) and not(contains(@name, '#ctor'))]" mode="method"/-->
				<xsl:apply-templates select="//member[starts-with(@name, $methodName) and not(contains(@name, '#ctor'))]" mode="method"/>
			</table>
		</xsl:if>

		<xsl:variable name="fieldName" select="concat('F:', $fName)"/>
		<xsl:variable name="fieldBody">
			<xsl:apply-templates select="//member[starts-with(@name, $fieldName)]" mode="field">
				<xsl:with-param name="fieldName" select="$fieldName"/>
			</xsl:apply-templates>
		</xsl:variable>
		<!--xsl:if test="//member[starts-with(@name, $fieldName)]"-->
		<xsl:if test="$fieldBody != ''">
			<h2>Fields</h2>
			<table cellpadding="0" cellspacing="0">
				<tr>
					<th>Name</th>
					<th>Description</th>
				</tr>
				<xsl:copy-of select="$fieldBody"/>
				<!--xsl:apply-templates select="//member[starts-with(@name, $fieldName)]" mode="field">
						<xsl:with-param name="fieldName" select="$fieldName"/>
					</xsl:apply-templates-->
			</table>
		</xsl:if>

		<xsl:variable name="eventName" select="concat('E:',$fName)"/>
		<xsl:if test="//member[starts-with(@name, $eventName)]">
			<h2>Events</h2>
			<table cellpadding="0" cellspacing="0">
				<tr>
					<th>Name</th>
					<th>Description</th>
				</tr>
				<xsl:apply-templates select="//member[starts-with(@name, $eventName)]" mode="event"/>
			</table>
		</xsl:if>
		<xsl:if test="remarks">
			<h2>Remarks</h2>
			<xsl:apply-templates select="remarks"/>
		</xsl:if>
		<xsl:if test="seealso">
			<h2>See Also</h2>
			<ul>
				<xsl:apply-templates select="seealso"/>
			</ul>
		</xsl:if>
		<hr/>
	</xsl:template>


	<xsl:template match="member" mode="constructor">
		<xsl:variable name="fName" select="substring-after(@name, 'M:')"/>
		<tr>
			<td>
				<a>
					<xsl:attribute name="name">
						<xsl:value-of select="$fName"/>
					</xsl:attribute>
				</a>
				<xsl:value-of select="concat(substring-before($fName, '.#'), substring-after($fName,'ctor'))"/>
			</td>
			<td>
				<xsl:apply-templates select="summary"/>
				<xsl:if test="param">
					<h3>Parameters</h3>
					<xsl:apply-templates select="param"/>
				</xsl:if>
				<xsl:apply-templates select="returns"/>
				<xsl:apply-templates select="remarks"/>
				<xsl:apply-templates select="example"/>
				&#160;
			</td>
		</tr>
	</xsl:template>

	<xsl:template match="member" mode="method">
		<xsl:variable name="fullName" select="substring-after(@name, 'M:')"/>
		<xsl:variable name="clean">
			<xsl:call-template name="CleanName">
				<xsl:with-param name="fullString" select="$fullName"/>
			</xsl:call-template>
		</xsl:variable>
		<tr>
			<td>
				<a>
					<xsl:attribute name="name">
						<xsl:value-of select="$fullName"/>
					</xsl:attribute>
				</a>
				<xsl:value-of select="$clean"/>
				<xsl:if test="contains($fullName,'(')">
					(<xsl:call-template name="addSpaces">
						<xsl:with-param name="s" select="substring-after($fullName,'(')"/>
					</xsl:call-template>
				</xsl:if>
			</td>
			<td>
				<xsl:apply-templates select="summary"/>
				<xsl:if test="param">
					<h3>Parameters</h3>
					<xsl:apply-templates select="param"/>
				</xsl:if>
				<xsl:apply-templates select="returns"/>
				<xsl:apply-templates select="remarks"/>
				<xsl:apply-templates select="example"/>
				<xsl:if test="exception">
					<h3>Exceptions</h3>
					<table cellpadding="0" cellspacing="0">
						<tr>
							<th>Exception</th>
							<th>Condition</th>
						</tr>
						<xsl:apply-templates select="exception"/>
					</table>
				</xsl:if>
			</td>
		</tr>
	</xsl:template>

	<xsl:template match="member" mode="property">
		<xsl:param name="propertyName"/>
		<xsl:variable name="foundName">
			<xsl:call-template name="findName">
				<xsl:with-param name="fName" select="substring-after(@name, 'P:')"/>
			</xsl:call-template>
		</xsl:variable>
		<xsl:if test="concat($propertyName, '.', $foundName) = @name">
			<tr>
				<td>
					<a>
						<xsl:attribute name="name">
							<xsl:value-of select="substring-after(@name, 'P:')"/>
						</xsl:attribute>
					</a>
					<xsl:value-of select="$foundName"/>
				</td>
				<td>
					<xsl:apply-templates/>
				</td>
			</tr>
		</xsl:if>
	</xsl:template>

	<xsl:template match="member" mode="field">
		<xsl:param name="fieldName"/>
		<xsl:variable name="foundName">
			<xsl:call-template name="findName">
				<xsl:with-param name="fName" select="substring-after(@name, 'F:')" />
			</xsl:call-template>
		</xsl:variable>
		<xsl:if test="concat($fieldName, '.', $foundName) = @name">
			<tr>
				<td>
					<a>
						<xsl:attribute name="name">
							<xsl:value-of select="substring-after(@name, 'F:')"/>
						</xsl:attribute>
					</a>
					<xsl:value-of select="$foundName"/>
				</td>
				<td>
					<xsl:apply-templates/>
				</td>
			</tr>
		</xsl:if>
	</xsl:template>

	<xsl:template match="member" mode="event">
		<xsl:variable name="foundName">
			<xsl:call-template name="findName">
				<xsl:with-param name="fName" select="substring-after(@name, 'E:')"/>
			</xsl:call-template>
		</xsl:variable>
		<tr>
			<td>
				<a>
					<xsl:attribute name="name">
						<xsl:value-of select="substring-after(@name, 'E:')"/>
					</xsl:attribute>
				</a>
				<xsl:value-of select="$foundName"/>
			</td>
			<td>
				<xsl:apply-templates/>
			</td>
		</tr>
	</xsl:template>

	<xsl:template match="remarks">
		<div>
			<xsl:apply-templates/>
		</div>
	</xsl:template>

	<xsl:template match="param">
		<dl>
			<dt>
				<xsl:value-of select="@name"/>
			</dt>
			<dd>
				<xsl:apply-templates/>
			</dd>
		</dl>
	</xsl:template>

	<xsl:template match="summary">
		<div>
			<xsl:apply-templates/>
		</div>
	</xsl:template>

	<xsl:template match="see">
		<xsl:variable name="fName" select="substring-after(@cref,':')"/>
		<xsl:variable name="clean">
			<xsl:call-template name="CleanName">
				<xsl:with-param name="fullString" select="$fName"/>
			</xsl:call-template>
		</xsl:variable>
		<a href="#{$fName}">
			<xsl:value-of select="$clean"/>
		</a>
	</xsl:template>

	<xsl:template match="seealso">
		<xsl:variable name="fName" select="substring-after(@cref,':')"/>
		<xsl:variable name="clean">
			<xsl:call-template name="CleanName">
				<xsl:with-param name="fullString" select="$fName"/>
			</xsl:call-template>
		</xsl:variable>
		<li>
			<a href="#{$fName}">
				<xsl:value-of select="$clean"/>
			</a>
		</li>
	</xsl:template>

	<xsl:template match="paramref">
		<i>
			<xsl:value-of select="@name"/>
		</i>
	</xsl:template>

	<xsl:template match="para">
		<p>
			<xsl:apply-templates/>
		</p>
	</xsl:template>

	<xsl:template match="note">
		<div class="note">
			<b>Note</b>&#160;&#160;<xsl:apply-templates/>
		</div>
	</xsl:template>

	<xsl:template match="list[@type='bullet']">
		<ul>
			<xsl:for-each select="item">
				<li>
					<xsl:apply-templates/>
				</li>
			</xsl:for-each>
		</ul>
	</xsl:template>

	<xsl:template match="b">
		<b>
			<xsl:value-of select="."/>
		</b>
	</xsl:template>

	<xsl:template match="returns">
		<div>
			Returns: <xsl:apply-templates/>
		</div>
	</xsl:template>

	<xsl:template match="example">
		<h3>Code Example</h3>
		<xsl:if test="code[@source]">
			<xsl:apply-templates select="para"/>
			<table cellpadding="0" cellspacing="0">
				<tr>
					<th>Title</th>
					<th>Path</th>
					<th>Lang</th>
				</tr>
				<xsl:apply-templates/>
			</table>
		</xsl:if>
		<xsl:apply-templates/>
	</xsl:template>

	<xsl:template match="code">
		<xsl:if test=". !=''">
			<pre>
				<xsl:value-of select="."/>
			</pre>
		</xsl:if>
	</xsl:template>

	<xsl:template match="code[@source]">
		<tr>
			<td>
				<xsl:value-of select="@title"/>
			</td>
			<td>
				<xsl:value-of select="@source"/>
			</td>
			<td>
				<xsl:value-of select="@lang"/>
			</td>
		</tr>
	</xsl:template>

	<xsl:template match="exception">
		<tr>
			<td>
				<xsl:value-of select="substring-after(@cref,'T:')"/>
			</td>
			<td>
				<xsl:apply-templates/>
			</td>
		</tr>

	</xsl:template>

	<!-- Named templates -->

	<xsl:template name="findName">
		<xsl:param name="fName"/>
		<xsl:variable name="newString" select="substring-after($fName,'.')"/>
		<xsl:choose>
			<xsl:when test="contains($newString,'.')">
				<xsl:call-template name="findName">
					<xsl:with-param name="fName" select="$newString"/>
				</xsl:call-template>
			</xsl:when>
			<xsl:otherwise>
				<xsl:value-of select="$newString"/>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>

	<xsl:template name="stringBeforeParens">
		<xsl:param name="fullString"/>
		<xsl:choose>
			<xsl:when test="contains($fullString,'(')">
				<xsl:value-of select="substring-before($fullString,'(')"/>
			</xsl:when>
			<xsl:otherwise>
				<xsl:value-of select="$fullString"/>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>

	<xsl:template name="CleanName">
		<xsl:param name="fullString"/>
		<xsl:variable name="beforeParens">
			<xsl:call-template name="stringBeforeParens">
				<xsl:with-param name="fullString" select="$fullString"/>
			</xsl:call-template>
		</xsl:variable>
		<xsl:variable name="foundName">
			<xsl:call-template name="findName">
				<xsl:with-param name="fName" select="$beforeParens"/>
			</xsl:call-template>
		</xsl:variable>
		<xsl:value-of select="$foundName"/>
	</xsl:template>

	<xsl:template name="addSpaces">
		<xsl:param name="s"/>
		<xsl:choose>
			<xsl:when test="contains($s,',')">
				<xsl:value-of select="substring-before($s,',')"/>,<xsl:text> </xsl:text>
				<xsl:call-template name="addSpaces">
					<xsl:with-param name="s" select="substring-after($s,',')"/>
				</xsl:call-template>
			</xsl:when>
			<xsl:otherwise>
				<xsl:value-of select="$s"/>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>

</xsl:stylesheet>
