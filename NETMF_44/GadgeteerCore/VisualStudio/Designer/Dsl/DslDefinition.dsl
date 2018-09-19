<?xml version="1.0" encoding="utf-8"?>
<Dsl xmlns:dm0="http://schemas.microsoft.com/VisualStudio/2008/DslTools/Core" dslVersion="1.0.0.0" Id="b860f50b-5cad-4f39-8f02-2e438befbe65" Description="Description for Microsoft.Gadgeteer.Designer.GadgeteerDSL" Name="GadgeteerDSL" DisplayName="GadgeteerDSL" Namespace="Microsoft.Gadgeteer.Designer" ProductName="Gadgeteer" CompanyName="Microsoft" PackageGuid="e627fec7-215a-4eed-a85e-440df3eb5f5f" PackageNamespace="Microsoft.Gadgeteer.Designer" xmlns="http://schemas.microsoft.com/VisualStudio/2005/DslTools/DslDefinitionModel">
  <Classes>
    <DomainClass Id="15ee651a-ee3e-4113-813d-0b14e94d5998" Description="The root in which all other elements are embedded. Appears as a diagram." Name="GadgeteerModel" DisplayName="Gadgeteer Model" Namespace="Microsoft.Gadgeteer.Designer" GeneratesDoubleDerived="true">
      <Properties>
        <DomainProperty Id="645f6ed3-7e77-47d5-a68a-32485cb438b7" Description="Description for Microsoft.Gadgeteer.Designer.GadgeteerModel.New Model" Name="NewModel" DisplayName="New Model" DefaultValue="true" IsBrowsable="false" IsUIReadOnly="true">
          <Type>
            <ExternalTypeMoniker Name="/System/Boolean" />
          </Type>
        </DomainProperty>
      </Properties>
      <ElementMergeDirectives>
        <ElementMergeDirective>
          <Index>
            <DomainClassMoniker Name="GadgeteerHardware" />
          </Index>
          <LinkCreationPaths>
            <DomainPath>GadgeteerModelHasGadgeteerHardware.GadgeteerHardware</DomainPath>
          </LinkCreationPaths>
        </ElementMergeDirective>
        <ElementMergeDirective>
          <Index>
            <DomainClassMoniker Name="Comment" />
          </Index>
          <LinkCreationPaths>
            <DomainPath>GadgeteerModelHasComments.Comments</DomainPath>
          </LinkCreationPaths>
        </ElementMergeDirective>
      </ElementMergeDirectives>
    </DomainClass>
    <DomainClass Id="23b7e991-e611-40e0-a03c-cf96b2f2027e" Description="Description for Microsoft.Gadgeteer.Designer.Mainboard" Name="Mainboard" DisplayName="Mainboard" Namespace="Microsoft.Gadgeteer.Designer" GeneratesDoubleDerived="true">
      <BaseClass>
        <DomainClassMoniker Name="GadgeteerHardware" />
      </BaseClass>
      <Properties>
        <DomainProperty Id="bdd1f61d-c295-43ee-9005-65d9db861c2e" Description="Description for Microsoft.Gadgeteer.Designer.Mainboard.Name" Name="Name" DisplayName="Name" IsElementName="true" IsUIReadOnly="true">
          <Type>
            <ExternalTypeMoniker Name="/System/String" />
          </Type>
        </DomainProperty>
      </Properties>
    </DomainClass>
    <DomainClass Id="60921340-ff2d-4314-b24b-55d56be869af" Description="Description for Microsoft.Gadgeteer.Designer.Module" Name="Module" DisplayName="Module" Namespace="Microsoft.Gadgeteer.Designer" GeneratesDoubleDerived="true">
      <BaseClass>
        <DomainClassMoniker Name="GadgeteerHardware" />
      </BaseClass>
      <Properties>
        <DomainProperty Id="01757930-f488-42ac-982a-b10bf283759a" Description="Description for Microsoft.Gadgeteer.Designer.Module.Name" Name="Name" DisplayName="Name" IsElementName="true">
          <Type>
            <ExternalTypeMoniker Name="/System/String" />
          </Type>
          <ElementNameProvider>
            <ExternalTypeMoniker Name="StartAtTwoNameProvider" />
          </ElementNameProvider>
        </DomainProperty>
        <DomainProperty Id="19e586e2-61f9-42dd-9a92-3759d311c3c1" Description="Description for Microsoft.Gadgeteer.Designer.Module.Module Type" Name="ModuleType" DisplayName="Module Type" IsBrowsable="false">
          <Type>
            <ExternalTypeMoniker Name="/System/String" />
          </Type>
        </DomainProperty>
        <DomainProperty Id="cf93e9a7-fc2d-446e-8e9f-730c8b214a43" Description="Description for Microsoft.Gadgeteer.Designer.Module.Module Definition Id" Name="ModuleDefinitionId" DisplayName="Module Definition Id" IsBrowsable="false">
          <Type>
            <ExternalTypeMoniker Name="/System/Guid" />
          </Type>
        </DomainProperty>
        <DomainProperty Id="473d00d5-232d-401c-a97b-7296403026ab" Description="Description for Microsoft.Gadgeteer.Designer.Module.Net Micro Framework Assembly" Name="NetMicroFrameworkAssembly" DisplayName="Net Micro Framework Assembly" IsBrowsable="false" IsUIReadOnly="true">
          <Type>
            <ExternalTypeMoniker Name="/System/String" />
          </Type>
        </DomainProperty>
        <DomainProperty Id="939635ec-c7db-49a1-9030-c036a790d1eb" Description="Description for Microsoft.Gadgeteer.Designer.Module.Manufacturer" Name="Manufacturer" DisplayName="Manufacturer" Kind="Calculated">
          <Type>
            <ExternalTypeMoniker Name="/System/String" />
          </Type>
        </DomainProperty>
        <DomainProperty Id="ca3848fd-3599-4b32-a380-6eb572e1bb91" Description="Description for Microsoft.Gadgeteer.Designer.Module.Type" Name="Type" DisplayName="Type" Kind="Calculated">
          <Type>
            <ExternalTypeMoniker Name="/System/String" />
          </Type>
        </DomainProperty>
      </Properties>
      <ElementMergeDirectives>
        <ElementMergeDirective>
          <Index>
            <DomainClassMoniker Name="SocketUse" />
          </Index>
          <LinkCreationPaths>
            <DomainPath>ModuleHasSocketUses.SocketUses</DomainPath>
          </LinkCreationPaths>
        </ElementMergeDirective>
      </ElementMergeDirectives>
    </DomainClass>
    <DomainClass Id="d01dde38-eb61-4956-9748-e28148f4c398" Description="Description for Microsoft.Gadgeteer.Designer.Socket" Name="Socket" DisplayName="Socket" Namespace="Microsoft.Gadgeteer.Designer">
      <BaseClass>
        <DomainClassMoniker Name="SocketBase" />
      </BaseClass>
      <Properties>
        <DomainProperty Id="8693394f-427e-4c40-bfc6-d3296ba5061b" Description="Description for Microsoft.Gadgeteer.Designer.Socket.Label" Name="Label" DisplayName="Label" IsElementName="true" IsUIReadOnly="true">
          <Type>
            <ExternalTypeMoniker Name="/System/String" />
          </Type>
        </DomainProperty>
      </Properties>
    </DomainClass>
    <DomainClass Id="cd8f841a-13ee-4cb9-b810-3874bfd91755" Description="Description for Microsoft.Gadgeteer.Designer.SocketUse" Name="SocketUse" DisplayName="Socket Use" Namespace="Microsoft.Gadgeteer.Designer">
      <BaseClass>
        <DomainClassMoniker Name="SocketBase" />
      </BaseClass>
      <Properties>
        <DomainProperty Id="e3c88a24-1daf-4d8e-89b8-efa80a2abe1d" Description="Description for Microsoft.Gadgeteer.Designer.SocketUse.Label" Name="Label" DisplayName="Label" IsElementName="true" IsUIReadOnly="true">
          <Type>
            <ExternalTypeMoniker Name="/System/String" />
          </Type>
        </DomainProperty>
        <DomainProperty Id="49bbf2d1-ea0c-45fd-9cd5-7bd802d30ea4" Description="Description for Microsoft.Gadgeteer.Designer.SocketUse.Optional" Name="Optional" DisplayName="Optional" Kind="Calculated" IsUIReadOnly="true">
          <Type>
            <ExternalTypeMoniker Name="/System/Boolean" />
          </Type>
        </DomainProperty>
      </Properties>
    </DomainClass>
    <DomainClass Id="75fd3552-c538-45c0-baf6-4555afa08458" Description="Description for Microsoft.Gadgeteer.Designer.GadgeteerHardware" Name="GadgeteerHardware" DisplayName="Gadgeteer Hardware" InheritanceModifier="Abstract" Namespace="Microsoft.Gadgeteer.Designer" GeneratesDoubleDerived="true">
      <Properties>
        <DomainProperty Id="ca915efb-f1a0-4746-b8f4-7f044483c1a3" Description="Description for Microsoft.Gadgeteer.Designer.GadgeteerHardware.Cached Definition" Name="CachedDefinition" DisplayName="Cached Definition" SetterAccessModifier="Assembly" IsBrowsable="false">
          <Type>
            <ExternalTypeMoniker Name="/System/String" />
          </Type>
        </DomainProperty>
      </Properties>
      <ElementMergeDirectives>
        <ElementMergeDirective>
          <Index>
            <DomainClassMoniker Name="Socket" />
          </Index>
          <LinkCreationPaths>
            <DomainPath>GadgeteerHardwareHasSockets.Sockets</DomainPath>
          </LinkCreationPaths>
        </ElementMergeDirective>
      </ElementMergeDirectives>
    </DomainClass>
    <DomainClass Id="c1960a79-77fe-4306-9b84-d3b5e785e75f" Description="Description for Microsoft.Gadgeteer.Designer.SocketBase" Name="SocketBase" DisplayName="Socket Base" InheritanceModifier="Abstract" Namespace="Microsoft.Gadgeteer.Designer" />
    <DomainClass Id="986d4bd7-4766-46d2-b9d4-db1f3eaac7c8" Description="" Name="Comment" DisplayName="Comment" Namespace="Microsoft.Gadgeteer.Designer">
      <Properties>
        <DomainProperty Id="c8cf4fa5-f9f0-42d5-83af-f7b4e47e1192" Description="" Name="Text" DisplayName="Text" DefaultValue="">
          <Type>
            <ExternalTypeMoniker Name="/System/String" />
          </Type>
        </DomainProperty>
      </Properties>
    </DomainClass>
  </Classes>
  <Relationships>
    <DomainRelationship Id="0843f0b4-9e7a-4692-bea5-20d73c0da26b" Description="Description for Microsoft.Gadgeteer.Designer.ModuleHasSocketUses" Name="ModuleHasSocketUses" DisplayName="Module Has Socket Uses" Namespace="Microsoft.Gadgeteer.Designer" IsEmbedding="true">
      <Source>
        <DomainRole Id="c45a062a-3111-4f57-a260-ce23fbdfbb5d" Description="Description for Microsoft.Gadgeteer.Designer.ModuleHasSocketUses.Module" Name="Module" DisplayName="Module" PropertyName="SocketUses" PropagatesCopy="PropagatesCopyToLinkAndOppositeRolePlayer" PropertyDisplayName="Socket Uses">
          <RolePlayer>
            <DomainClassMoniker Name="Module" />
          </RolePlayer>
        </DomainRole>
      </Source>
      <Target>
        <DomainRole Id="e787aed6-bc16-4176-ac01-9699ac05e44e" Description="Description for Microsoft.Gadgeteer.Designer.ModuleHasSocketUses.SocketUse" Name="SocketUse" DisplayName="Socket Use" PropertyName="Module" Multiplicity="One" PropagatesDelete="true" PropertyDisplayName="Module">
          <RolePlayer>
            <DomainClassMoniker Name="SocketUse" />
          </RolePlayer>
        </DomainRole>
      </Target>
    </DomainRelationship>
    <DomainRelationship Id="d100822f-5c6f-48c1-9453-7d744ec6067b" Description="Description for Microsoft.Gadgeteer.Designer.SocketUseReferencesSocket" Name="SocketUseReferencesSocket" DisplayName="Socket Use References Socket" Namespace="Microsoft.Gadgeteer.Designer">
      <Source>
        <DomainRole Id="a67bb246-2606-48ed-87d9-19f42bae6b69" Description="Description for Microsoft.Gadgeteer.Designer.SocketUseReferencesSocket.SocketUse" Name="SocketUse" DisplayName="Socket Use" PropertyName="Socket" Multiplicity="One" PropertyDisplayName="Socket">
          <RolePlayer>
            <DomainClassMoniker Name="SocketUse" />
          </RolePlayer>
        </DomainRole>
      </Source>
      <Target>
        <DomainRole Id="97dd8fc5-90ab-4352-b985-6b9a0e8e7308" Description="Description for Microsoft.Gadgeteer.Designer.SocketUseReferencesSocket.Socket" Name="Socket" DisplayName="Socket" PropertyName="SocketUse" Multiplicity="One" PropertyDisplayName="Socket Use">
          <RolePlayer>
            <DomainClassMoniker Name="Socket" />
          </RolePlayer>
        </DomainRole>
      </Target>
    </DomainRelationship>
    <DomainRelationship Id="1889ca25-4789-4e56-9489-170e21757848" Description="Description for Microsoft.Gadgeteer.Designer.GadgeteerModelHasGadgeteerHardware" Name="GadgeteerModelHasGadgeteerHardware" DisplayName="Gadgeteer Model Has Gadgeteer Hardware" Namespace="Microsoft.Gadgeteer.Designer" IsEmbedding="true">
      <Source>
        <DomainRole Id="24f42c48-104f-40e9-940b-6db3f052a013" Description="Description for Microsoft.Gadgeteer.Designer.GadgeteerModelHasGadgeteerHardware.GadgeteerModel" Name="GadgeteerModel" DisplayName="Gadgeteer Model" PropertyName="GadgeteerHardware" PropagatesCopy="PropagatesCopyToLinkAndOppositeRolePlayer" PropertyDisplayName="Gadgeteer Hardware">
          <RolePlayer>
            <DomainClassMoniker Name="GadgeteerModel" />
          </RolePlayer>
        </DomainRole>
      </Source>
      <Target>
        <DomainRole Id="546995b5-0dd2-41e4-a039-61874fe4ed9d" Description="Description for Microsoft.Gadgeteer.Designer.GadgeteerModelHasGadgeteerHardware.GadgeteerHardware" Name="GadgeteerHardware" DisplayName="Gadgeteer Hardware" PropertyName="GadgeteerModel" Multiplicity="One" PropagatesDelete="true" PropertyDisplayName="Gadgeteer Model">
          <RolePlayer>
            <DomainClassMoniker Name="GadgeteerHardware" />
          </RolePlayer>
        </DomainRole>
      </Target>
    </DomainRelationship>
    <DomainRelationship Id="f79ba18d-1935-41ed-b7b2-3ec88740823c" Description="Description for Microsoft.Gadgeteer.Designer.GadgeteerHardwareHasSockets" Name="GadgeteerHardwareHasSockets" DisplayName="Gadgeteer Hardware Has Sockets" Namespace="Microsoft.Gadgeteer.Designer" IsEmbedding="true">
      <Source>
        <DomainRole Id="fd7faa8c-09a2-40e0-bf4a-b31236856b7b" Description="Description for Microsoft.Gadgeteer.Designer.GadgeteerHardwareHasSockets.GadgeteerHardware" Name="GadgeteerHardware" DisplayName="Gadgeteer Hardware" PropertyName="Sockets" PropagatesCopy="PropagatesCopyToLinkAndOppositeRolePlayer" PropertyDisplayName="Sockets">
          <RolePlayer>
            <DomainClassMoniker Name="GadgeteerHardware" />
          </RolePlayer>
        </DomainRole>
      </Source>
      <Target>
        <DomainRole Id="8a450cb2-6aa7-41bf-a495-62696beb06af" Description="Description for Microsoft.Gadgeteer.Designer.GadgeteerHardwareHasSockets.Socket" Name="Socket" DisplayName="Socket" PropertyName="GadgeteerHardware" Multiplicity="One" PropagatesDelete="true" PropertyDisplayName="Gadgeteer Hardware">
          <RolePlayer>
            <DomainClassMoniker Name="Socket" />
          </RolePlayer>
        </DomainRole>
      </Target>
    </DomainRelationship>
    <DomainRelationship Id="24dd469b-78ec-4006-9774-c4fb5ae2dd1b" Description="Description for Microsoft.Gadgeteer.Designer.GadgeteerModelHasComments" Name="GadgeteerModelHasComments" DisplayName="Gadgeteer Model Has Comments" Namespace="Microsoft.Gadgeteer.Designer" IsEmbedding="true">
      <Source>
        <DomainRole Id="7e215ff5-b9d3-4884-82f2-cd2cdd897f5a" Description="Description for Microsoft.Gadgeteer.Designer.GadgeteerModelHasComments.GadgeteerModel" Name="GadgeteerModel" DisplayName="Gadgeteer Model" PropertyName="Comments" PropagatesCopy="PropagatesCopyToLinkAndOppositeRolePlayer" PropertyDisplayName="Comments">
          <RolePlayer>
            <DomainClassMoniker Name="GadgeteerModel" />
          </RolePlayer>
        </DomainRole>
      </Source>
      <Target>
        <DomainRole Id="89705449-b1a9-4b00-977c-36ab490ead3a" Description="Description for Microsoft.Gadgeteer.Designer.GadgeteerModelHasComments.Comment" Name="Comment" DisplayName="Comment" PropertyName="GadgeteerModel" Multiplicity="One" PropagatesDelete="true" PropertyDisplayName="Gadgeteer Model">
          <RolePlayer>
            <DomainClassMoniker Name="Comment" />
          </RolePlayer>
        </DomainRole>
      </Target>
    </DomainRelationship>
  </Relationships>
  <Types>
    <ExternalType Name="DateTime" Namespace="System" />
    <ExternalType Name="String" Namespace="System" />
    <ExternalType Name="Int16" Namespace="System" />
    <ExternalType Name="Int32" Namespace="System" />
    <ExternalType Name="Int64" Namespace="System" />
    <ExternalType Name="UInt16" Namespace="System" />
    <ExternalType Name="UInt32" Namespace="System" />
    <ExternalType Name="UInt64" Namespace="System" />
    <ExternalType Name="SByte" Namespace="System" />
    <ExternalType Name="Byte" Namespace="System" />
    <ExternalType Name="Double" Namespace="System" />
    <ExternalType Name="Single" Namespace="System" />
    <ExternalType Name="Guid" Namespace="System" />
    <ExternalType Name="Boolean" Namespace="System" />
    <ExternalType Name="Char" Namespace="System" />
    <ExternalType Name="StartAtTwoNameProvider" Namespace="Microsoft.Gadgeteer.Designer" />
  </Types>
  <Shapes>
    <GeometryShape Id="e7cd5164-7930-4df2-861b-868b10ba4bc6" Description="Description for Microsoft.Gadgeteer.Designer.GadgeteerHardwareShape" Name="GadgeteerHardwareShape" DisplayName="Gadgeteer Hardware Shape" Namespace="Microsoft.Gadgeteer.Designer" GeneratesDoubleDerived="true" FixedTooltipText="Gadgeteer Hardware Shape" FillColor="64, 64, 64" InitialWidth="2" InitialHeight="2" FillGradientMode="ForwardDiagonal" Geometry="RoundedRectangle" />
    <GeometryShape Id="d8c60177-c21c-494c-9f65-e8773bdd3b21" Description="Description for Microsoft.Gadgeteer.Designer.SocketShape" Name="SocketShape" DisplayName="Socket Shape" Namespace="Microsoft.Gadgeteer.Designer" FixedTooltipText="Socket Shape" TextColor="White" FillColor="Transparent" OutlineColor="Transparent" InitialWidth="0.25" InitialHeight="0.25" OutlineThickness="0.02125" FillGradientMode="None" Geometry="Circle">
      <BaseGeometryShape>
        <GeometryShapeMoniker Name="SocketShapeBase" />
      </BaseGeometryShape>
      <ShapeHasDecorators Position="Center" HorizontalOffset="0" VerticalOffset="0">
        <TextDecorator Name="NameDecorator" DisplayName="Name Decorator" DefaultText="NameDecorator" FontStyle="Bold" FontSize="10" />
      </ShapeHasDecorators>
    </GeometryShape>
    <GeometryShape Id="846e3255-fcf7-4232-867c-ab6ec4472d5c" Description="Description for Microsoft.Gadgeteer.Designer.ModuleShape" Name="ModuleShape" DisplayName="Module Shape" Namespace="Microsoft.Gadgeteer.Designer" FixedTooltipText="Module Shape" FillColor="64, 64, 64" InitialWidth="0.8" InitialHeight="0.8" FillGradientMode="None" Geometry="RoundedRectangle">
      <BaseGeometryShape>
        <GeometryShapeMoniker Name="GadgeteerHardwareShape" />
      </BaseGeometryShape>
      <ShapeHasDecorators Position="OuterBottomCenter" HorizontalOffset="0" VerticalOffset="0">
        <TextDecorator Name="LabelDecorator" DisplayName="Label Decorator" DefaultText="LabelDecorator" />
      </ShapeHasDecorators>
      <ShapeHasDecorators Position="OuterTopCenter" HorizontalOffset="0" VerticalOffset="0">
        <TextDecorator Name="TypeDecorator" DisplayName="Type Decorator" DefaultText="TypeDecorator" FontSize="7" />
      </ShapeHasDecorators>
    </GeometryShape>
    <GeometryShape Id="eec2b86d-7de3-4560-ae8d-c0f843665b3b" Description="Description for Microsoft.Gadgeteer.Designer.MainboardShape" Name="MainboardShape" DisplayName="Mainboard Shape" Namespace="Microsoft.Gadgeteer.Designer" FixedTooltipText="Mainboard Shape" FillColor="64, 64, 64" InitialHeight="1" FillGradientMode="None" Geometry="RoundedRectangle">
      <BaseGeometryShape>
        <GeometryShapeMoniker Name="GadgeteerHardwareShape" />
      </BaseGeometryShape>
    </GeometryShape>
    <GeometryShape Id="73ddc30e-9897-4662-b06c-95a13807d806" Description="Description for Microsoft.Gadgeteer.Designer.SocketUseShape" Name="SocketUseShape" DisplayName="Socket Use Shape" Namespace="Microsoft.Gadgeteer.Designer" FixedTooltipText="Socket Use Shape" TextColor="White" FillColor="Transparent" OutlineColor="Transparent" InitialWidth="0.25" InitialHeight="0.25" OutlineThickness="0.02125" FillGradientMode="None" Geometry="Circle">
      <BaseGeometryShape>
        <GeometryShapeMoniker Name="SocketShapeBase" />
      </BaseGeometryShape>
    </GeometryShape>
    <GeometryShape Id="238fc2c2-9f8f-41a6-9703-541d0f10db1c" Description="Description for Microsoft.Gadgeteer.Designer.SocketShapeBase" Name="SocketShapeBase" DisplayName="Socket Shape Base" Namespace="Microsoft.Gadgeteer.Designer" FixedTooltipText="Socket Shape Base" FillColor="Transparent" InitialWidth="0.2" InitialHeight="0.2" FillGradientMode="None" Geometry="Circle" />
    <GeometryShape Id="c1317624-bfaa-4a3c-b46f-7467e4fd2ad3" Description="" Name="CommentBoxShape" DisplayName="Comment Box Shape" Namespace="Microsoft.Gadgeteer.Designer" GeneratesDoubleDerived="true" FixedTooltipText="Comment Box Shape" FillColor="Khaki" OutlineColor="Tan" InitialWidth="1.75" InitialHeight="1.4" Geometry="Rectangle">
      <ShapeHasDecorators Position="InnerMiddleLeft" HorizontalOffset="0.5" VerticalOffset="0">
        <TextDecorator Name="Comment" DisplayName="Comment" DefaultText="" />
      </ShapeHasDecorators>
    </GeometryShape>
  </Shapes>
  <Connectors>
    <Connector Id="617632a1-6abc-4627-903b-a67c50a3fd74" Description="Socket connection" Name="SocketConnector" DisplayName="Socket Connector" Namespace="Microsoft.Gadgeteer.Designer" FixedTooltipText="Socket Connector" TextColor="DimGray" Color="0, 186, 255" Thickness="0.02" sourceEndWidth="0.05" sourceEndHeight="0.05" targetEndWidth="0.05" targetEndHeight="0.05" />
  </Connectors>
  <XmlSerializationBehavior Name="GadgeteerDSLSerializationBehavior" Namespace="Microsoft.Gadgeteer.Designer">
    <ClassData>
      <XmlClassData TypeName="GadgeteerModel" MonikerAttributeName="" SerializeId="true" MonikerElementName="gadgeteerModelMoniker" ElementName="gadgeteerModel" MonikerTypeName="GadgeteerModelMoniker">
        <DomainClassMoniker Name="GadgeteerModel" />
        <ElementData>
          <XmlRelationshipData UseFullForm="true" RoleElementName="gadgeteerHardware">
            <DomainRelationshipMoniker Name="GadgeteerModelHasGadgeteerHardware" />
          </XmlRelationshipData>
          <XmlRelationshipData UseFullForm="true" RoleElementName="comments">
            <DomainRelationshipMoniker Name="GadgeteerModelHasComments" />
          </XmlRelationshipData>
          <XmlPropertyData XmlName="newModel">
            <DomainPropertyMoniker Name="GadgeteerModel/NewModel" />
          </XmlPropertyData>
        </ElementData>
      </XmlClassData>
      <XmlClassData TypeName="GadgeteerDSLDiagram" MonikerAttributeName="" SerializeId="true" MonikerElementName="gadgeteerDSLDiagramMoniker" ElementName="gadgeteerDSLDiagram" MonikerTypeName="GadgeteerDSLDiagramMoniker">
        <DiagramMoniker Name="GadgeteerDSLDiagram" />
      </XmlClassData>
      <XmlClassData TypeName="Mainboard" MonikerAttributeName="" SerializeId="true" MonikerElementName="mainboardMoniker" ElementName="mainboard" MonikerTypeName="MainboardMoniker">
        <DomainClassMoniker Name="Mainboard" />
        <ElementData>
          <XmlPropertyData XmlName="name">
            <DomainPropertyMoniker Name="Mainboard/Name" />
          </XmlPropertyData>
        </ElementData>
      </XmlClassData>
      <XmlClassData TypeName="Module" MonikerAttributeName="" SerializeId="true" MonikerElementName="moduleMoniker" ElementName="module" MonikerTypeName="ModuleMoniker">
        <DomainClassMoniker Name="Module" />
        <ElementData>
          <XmlPropertyData XmlName="name">
            <DomainPropertyMoniker Name="Module/Name" />
          </XmlPropertyData>
          <XmlPropertyData XmlName="moduleType">
            <DomainPropertyMoniker Name="Module/ModuleType" />
          </XmlPropertyData>
          <XmlPropertyData XmlName="moduleDefinitionId">
            <DomainPropertyMoniker Name="Module/ModuleDefinitionId" />
          </XmlPropertyData>
          <XmlPropertyData XmlName="netMicroFrameworkAssembly">
            <DomainPropertyMoniker Name="Module/NetMicroFrameworkAssembly" />
          </XmlPropertyData>
          <XmlRelationshipData UseFullForm="true" RoleElementName="socketUses">
            <DomainRelationshipMoniker Name="ModuleHasSocketUses" />
          </XmlRelationshipData>
          <XmlPropertyData XmlName="manufacturer" Representation="Ignore">
            <DomainPropertyMoniker Name="Module/Manufacturer" />
          </XmlPropertyData>
          <XmlPropertyData XmlName="type" Representation="Ignore">
            <DomainPropertyMoniker Name="Module/Type" />
          </XmlPropertyData>
        </ElementData>
      </XmlClassData>
      <XmlClassData TypeName="Socket" MonikerAttributeName="" SerializeId="true" MonikerElementName="socketMoniker" ElementName="socket" MonikerTypeName="SocketMoniker">
        <DomainClassMoniker Name="Socket" />
        <ElementData>
          <XmlPropertyData XmlName="label">
            <DomainPropertyMoniker Name="Socket/Label" />
          </XmlPropertyData>
        </ElementData>
      </XmlClassData>
      <XmlClassData TypeName="GadgeteerHardwareShape" MonikerAttributeName="" SerializeId="true" MonikerElementName="gadgeteerHardwareShapeMoniker" ElementName="gadgeteerHardwareShape" MonikerTypeName="GadgeteerHardwareShapeMoniker">
        <GeometryShapeMoniker Name="GadgeteerHardwareShape" />
      </XmlClassData>
      <XmlClassData TypeName="SocketShape" MonikerAttributeName="" SerializeId="true" MonikerElementName="socketShapeMoniker" ElementName="socketShape" MonikerTypeName="SocketShapeMoniker">
        <GeometryShapeMoniker Name="SocketShape" />
      </XmlClassData>
      <XmlClassData TypeName="SocketUse" MonikerAttributeName="" SerializeId="true" MonikerElementName="socketUseMoniker" ElementName="socketUse" MonikerTypeName="SocketUseMoniker">
        <DomainClassMoniker Name="SocketUse" />
        <ElementData>
          <XmlPropertyData XmlName="label">
            <DomainPropertyMoniker Name="SocketUse/Label" />
          </XmlPropertyData>
          <XmlRelationshipData UseFullForm="true" RoleElementName="socket">
            <DomainRelationshipMoniker Name="SocketUseReferencesSocket" />
          </XmlRelationshipData>
          <XmlPropertyData XmlName="optional" Representation="Ignore">
            <DomainPropertyMoniker Name="SocketUse/Optional" />
          </XmlPropertyData>
        </ElementData>
      </XmlClassData>
      <XmlClassData TypeName="ModuleHasSocketUses" MonikerAttributeName="" SerializeId="true" MonikerElementName="moduleHasSocketUsesMoniker" ElementName="moduleHasSocketUses" MonikerTypeName="ModuleHasSocketUsesMoniker">
        <DomainRelationshipMoniker Name="ModuleHasSocketUses" />
      </XmlClassData>
      <XmlClassData TypeName="SocketUseReferencesSocket" MonikerAttributeName="" SerializeId="true" MonikerElementName="socketUseReferencesSocketMoniker" ElementName="socketUseReferencesSocket" MonikerTypeName="SocketUseReferencesSocketMoniker">
        <DomainRelationshipMoniker Name="SocketUseReferencesSocket" />
      </XmlClassData>
      <XmlClassData TypeName="GadgeteerHardware" MonikerAttributeName="" SerializeId="true" MonikerElementName="gadgeteerHardwareMoniker" ElementName="gadgeteerHardware" MonikerTypeName="GadgeteerHardwareMoniker">
        <DomainClassMoniker Name="GadgeteerHardware" />
        <ElementData>
          <XmlRelationshipData UseFullForm="true" RoleElementName="sockets">
            <DomainRelationshipMoniker Name="GadgeteerHardwareHasSockets" />
          </XmlRelationshipData>
          <XmlPropertyData XmlName="cachedDefinition">
            <DomainPropertyMoniker Name="GadgeteerHardware/CachedDefinition" />
          </XmlPropertyData>
        </ElementData>
      </XmlClassData>
      <XmlClassData TypeName="GadgeteerModelHasGadgeteerHardware" MonikerAttributeName="" SerializeId="true" MonikerElementName="gadgeteerModelHasGadgeteerHardwareMoniker" ElementName="gadgeteerModelHasGadgeteerHardware" MonikerTypeName="GadgeteerModelHasGadgeteerHardwareMoniker">
        <DomainRelationshipMoniker Name="GadgeteerModelHasGadgeteerHardware" />
      </XmlClassData>
      <XmlClassData TypeName="GadgeteerHardwareHasSockets" MonikerAttributeName="" SerializeId="true" MonikerElementName="gadgeteerHardwareHasSocketsMoniker" ElementName="gadgeteerHardwareHasSockets" MonikerTypeName="GadgeteerHardwareHasSocketsMoniker">
        <DomainRelationshipMoniker Name="GadgeteerHardwareHasSockets" />
      </XmlClassData>
      <XmlClassData TypeName="ModuleShape" MonikerAttributeName="" SerializeId="true" MonikerElementName="moduleShapeMoniker" ElementName="moduleShape" MonikerTypeName="ModuleShapeMoniker">
        <GeometryShapeMoniker Name="ModuleShape" />
      </XmlClassData>
      <XmlClassData TypeName="MainboardShape" MonikerAttributeName="" SerializeId="true" MonikerElementName="mainboardShapeMoniker" ElementName="mainboardShape" MonikerTypeName="MainboardShapeMoniker">
        <GeometryShapeMoniker Name="MainboardShape" />
      </XmlClassData>
      <XmlClassData TypeName="SocketUseShape" MonikerAttributeName="" SerializeId="true" MonikerElementName="socketUseShapeMoniker" ElementName="socketUseShape" MonikerTypeName="SocketUseShapeMoniker">
        <GeometryShapeMoniker Name="SocketUseShape" />
      </XmlClassData>
      <XmlClassData TypeName="SocketConnector" MonikerAttributeName="" SerializeId="true" MonikerElementName="socketConnectorMoniker" ElementName="socketConnector" MonikerTypeName="SocketConnectorMoniker">
        <ConnectorMoniker Name="SocketConnector" />
      </XmlClassData>
      <XmlClassData TypeName="SocketShapeBase" MonikerAttributeName="" SerializeId="true" MonikerElementName="socketShapeBaseMoniker" ElementName="socketShapeBase" MonikerTypeName="SocketShapeBaseMoniker">
        <GeometryShapeMoniker Name="SocketShapeBase" />
      </XmlClassData>
      <XmlClassData TypeName="SocketBase" MonikerAttributeName="" SerializeId="true" MonikerElementName="socketBaseMoniker" ElementName="socketBase" MonikerTypeName="SocketBaseMoniker">
        <DomainClassMoniker Name="SocketBase" />
      </XmlClassData>
      <XmlClassData TypeName="Comment" MonikerAttributeName="" SerializeId="true" MonikerElementName="commentMoniker" ElementName="comment" MonikerTypeName="CommentMoniker">
        <DomainClassMoniker Name="Comment" />
        <ElementData>
          <XmlPropertyData XmlName="text">
            <DomainPropertyMoniker Name="Comment/Text" />
          </XmlPropertyData>
        </ElementData>
      </XmlClassData>
      <XmlClassData TypeName="GadgeteerModelHasComments" MonikerAttributeName="" SerializeId="true" MonikerElementName="gadgeteerModelHasCommentsMoniker" ElementName="gadgeteerModelHasComments" MonikerTypeName="GadgeteerModelHasCommentsMoniker">
        <DomainRelationshipMoniker Name="GadgeteerModelHasComments" />
      </XmlClassData>
      <XmlClassData TypeName="CommentBoxShape" MonikerAttributeName="" SerializeId="true" MonikerElementName="commentBoxShapeMoniker" ElementName="commentBoxShape" MonikerTypeName="CommentBoxShapeMoniker">
        <GeometryShapeMoniker Name="CommentBoxShape" />
      </XmlClassData>
    </ClassData>
  </XmlSerializationBehavior>
  <ExplorerBehavior Name="GadgeteerDSLExplorer" />
  <ConnectionBuilders>
    <ConnectionBuilder Name="SocketUseReferencesSocketBuilder" IsCustom="true">
      <LinkConnectDirective UsesCustomConnect="true">
        <DomainRelationshipMoniker Name="SocketUseReferencesSocket" />
        <SourceDirectives>
          <RolePlayerConnectDirective>
            <AcceptingClass>
              <DomainClassMoniker Name="SocketUse" />
            </AcceptingClass>
          </RolePlayerConnectDirective>
        </SourceDirectives>
        <TargetDirectives>
          <RolePlayerConnectDirective UsesRoleSpecificCustomAccept="true">
            <AcceptingClass>
              <DomainClassMoniker Name="Socket" />
            </AcceptingClass>
          </RolePlayerConnectDirective>
        </TargetDirectives>
      </LinkConnectDirective>
    </ConnectionBuilder>
  </ConnectionBuilders>
  <Diagram Id="2b170eed-5601-40fe-aadd-7f146681fe46" Description="Description for Microsoft.Gadgeteer.Designer.GadgeteerDSLDiagram" Name="GadgeteerDSLDiagram" DisplayName="Minimal Language Diagram" Namespace="Microsoft.Gadgeteer.Designer" GeneratesDoubleDerived="true">
    <Class>
      <DomainClassMoniker Name="GadgeteerModel" />
    </Class>
    <ShapeMaps>
      <ShapeMap HasCustomParentElement="true">
        <DomainClassMoniker Name="Socket" />
        <ParentElementPath>
          <DomainPath />
        </ParentElementPath>
        <DecoratorMap>
          <TextDecoratorMoniker Name="SocketShape/NameDecorator" />
          <PropertyDisplayed>
            <PropertyPath>
              <DomainPropertyMoniker Name="Socket/Label" />
            </PropertyPath>
          </PropertyDisplayed>
        </DecoratorMap>
        <GeometryShapeMoniker Name="SocketShape" />
      </ShapeMap>
      <ShapeMap>
        <DomainClassMoniker Name="Module" />
        <ParentElementPath>
          <DomainPath>GadgeteerModelHasGadgeteerHardware.GadgeteerModel/!GadgeteerModel</DomainPath>
        </ParentElementPath>
        <DecoratorMap>
          <TextDecoratorMoniker Name="ModuleShape/LabelDecorator" />
          <PropertyDisplayed>
            <PropertyPath>
              <DomainPropertyMoniker Name="Module/Name" />
            </PropertyPath>
          </PropertyDisplayed>
        </DecoratorMap>
        <DecoratorMap>
          <TextDecoratorMoniker Name="ModuleShape/TypeDecorator" />
          <PropertyDisplayed>
            <PropertyPath>
              <DomainPropertyMoniker Name="Module/Type" />
            </PropertyPath>
          </PropertyDisplayed>
        </DecoratorMap>
        <GeometryShapeMoniker Name="ModuleShape" />
      </ShapeMap>
      <ShapeMap>
        <DomainClassMoniker Name="Mainboard" />
        <ParentElementPath>
          <DomainPath>GadgeteerModelHasGadgeteerHardware.GadgeteerModel/!GadgeteerModel</DomainPath>
        </ParentElementPath>
        <GeometryShapeMoniker Name="MainboardShape" />
      </ShapeMap>
      <ShapeMap HasCustomParentElement="true">
        <DomainClassMoniker Name="SocketUse" />
        <ParentElementPath>
          <DomainPath>ModuleHasSocketUses.Module/!Module/GadgeteerModelHasGadgeteerHardware.GadgeteerModel/!GadgeteerModel</DomainPath>
        </ParentElementPath>
        <GeometryShapeMoniker Name="SocketUseShape" />
      </ShapeMap>
      <ShapeMap>
        <DomainClassMoniker Name="Comment" />
        <ParentElementPath>
          <DomainPath>GadgeteerModelHasComments.GadgeteerModel/!GadgeteerModel</DomainPath>
        </ParentElementPath>
        <DecoratorMap>
          <TextDecoratorMoniker Name="CommentBoxShape/Comment" />
          <PropertyDisplayed>
            <PropertyPath>
              <DomainPropertyMoniker Name="Comment/Text" />
            </PropertyPath>
          </PropertyDisplayed>
        </DecoratorMap>
        <GeometryShapeMoniker Name="CommentBoxShape" />
      </ShapeMap>
    </ShapeMaps>
    <ConnectorMaps>
      <ConnectorMap>
        <ConnectorMoniker Name="SocketConnector" />
        <DomainRelationshipMoniker Name="SocketUseReferencesSocket" />
      </ConnectorMap>
    </ConnectorMaps>
  </Diagram>
  <Designer CopyPasteGeneration="CopyPasteOnly" FileExtension="gadgeteer" EditorGuid="b306cb57-56a9-4c9c-bc13-c489ee1c1d34">
    <RootClass>
      <DomainClassMoniker Name="GadgeteerModel" />
    </RootClass>
    <XmlSerializationDefinition CustomPostLoad="false">
      <XmlSerializationBehaviorMoniker Name="GadgeteerDSLSerializationBehavior" />
    </XmlSerializationDefinition>
    <ToolboxTab TabText="Gadgeteer">
      <ConnectionTool Name="SocketConnectionTool" ToolboxIcon="Resources\ExampleConnectorToolBitmap.bmp" Caption="Socket Connection" Tooltip="Socket Connection Tool" HelpKeyword="SocketConnectionTool">
        <ConnectionBuilderMoniker Name="GadgeteerDSL/SocketUseReferencesSocketBuilder" />
      </ConnectionTool>
      <ElementTool Name="Comment" ToolboxIcon="resources\CommentTool.bmp" Caption="Comment" Tooltip="Create a Comment" HelpKeyword="ConnectCommentF1Keyword">
        <DomainClassMoniker Name="Comment" />
      </ElementTool>
    </ToolboxTab>
    <Validation UsesMenu="false" UsesOpen="false" UsesSave="false" UsesLoad="false" />
    <DiagramMoniker Name="GadgeteerDSLDiagram" />
  </Designer>
  <Explorer ExplorerGuid="8c66aea6-d9d2-414a-9dc4-8ce2f99c952c" Title="Gadgeteer Model Explorer">
    <ExplorerBehaviorMoniker Name="GadgeteerDSL/GadgeteerDSLExplorer" />
  </Explorer>
</Dsl>