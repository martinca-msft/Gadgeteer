// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the Apache v.2 license.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.Gadgeteer.HardwareTemplateWizard
{
    static internal class DefaultXml
    {
        internal const string DefaultModuleSocketsXml = @"<!-- This is an example socket specification with two sockets on the board. -->
      <Sockets>

        <!-- This example socket is compatible with socket types XY which has electrical connections to pins 3 and 4 -->
        <Socket Left=""10"" Top=""10"" Orientation=""90"" ConstructorOrder=""1"" TypesLabel=""XY"">
          <Types>
            <Type>X</Type>
            <Type>Y</Type>
          </Types>
          <Pins>
            <Pin Shared=""false"">3</Pin>
            <Pin Shared=""false"">4</Pin>
          </Pins>
        </Socket>

        <!-- This example socket is compatible with socket type S, it is optional, and it has electrical connections to pins 3,4,5,7,8,9, with 7,8,9 being shareable (SPI bus) -->
        <Socket Left=""30"" Top=""10"" Orientation=""90"" ConstructorOrder=""2"" TypesLabel=""S"" Optional=""true"">
          <Types>
            <Type>S</Type>
          </Types>
          <Pins>
            <Pin Shared=""false"">3</Pin>
            <Pin Shared=""false"">4</Pin>
            <Pin Shared=""false"">5</Pin>
            <Pin Shared=""true"">7</Pin>
            <Pin Shared=""true"">8</Pin>
            <Pin Shared=""true"">9</Pin>
          </Pins>
        </Socket>

      </Sockets>";

        internal const string DefaultMainboardProvidedSocketsXml = @"<ProvidedSockets>

        <ProvidedSocket Label=""1"" Left=""10"" Top=""20"" Orientation=""90"" ConstructorParameter=""1"">
          <Types>
            <Type>A</Type>
            <Type>I</Type>
            <Type>X</Type>
          </Types>
          <!-- If mainboard sockets share pins (e.g. for I2C, SPI, or just shared GPIOs), then this should be specified using the SharedPinMaps element as for I2C below. -->
          <!-- Specify SharedOnly = ""true"" if the pin cannot be used in a non-shared fashion, e.g. for I2C the pins have pull-ups on the mainboard so they should not be used as general GPIOs. -->
          <SharedPinMaps>
            <SharedPinMap NetId=""I2CSDA"" SocketPin=""8"" SharedOnly=""true""/>
            <SharedPinMap NetId=""I2CSCL"" SocketPin=""9"" SharedOnly=""true""/>
          </SharedPinMaps>
        </ProvidedSocket>

        <ProvidedSocket Label=""2"" Left=""30"" Top=""20"" Orientation=""90"" ConstructorParameter=""2"">
          <Types>
            <Type>D</Type>
            <Type>I</Type>
          </Types>
          <SharedPinMaps>
            <SharedPinMap NetId=""I2CSDA"" SocketPin=""8"" SharedOnly=""true""/>
            <SharedPinMap NetId=""I2CSCL"" SocketPin=""9"" SharedOnly=""true""/>
          </SharedPinMaps>
        </ProvidedSocket>

      </ProvidedSockets>";

        internal const string DefaultModuleProvidedSocketsXml = @"<!-- The example below is for DaisyLink modules which provide a socket type ""*"" -->
      <!--
      <ProvidedSockets>
        <ProvidedSocket Label=""*"" Left=""30"" Top=""10"" Orientation=""0"" ConstructorParameter=""DaisyLinkSocketNumber"">
          <Types>
            <Type>*</Type>
          </Types>
        </ProvidedSocket>
      </ProvidedSockets>
      -->";

        internal const string DefaultPowerXml = @"<!--
      <Power>
        <PowerRequirements Voltage=""3.3"">
          <TypicalCurrent>0.123</TypicalCurrent>
          <MaximumCurrent>0.456</MaximumCurrent>
        </PowerRequirements>
        <PowerRequirements Voltage=""5.0"">
          <TypicalCurrent>0.789</TypicalCurrent>
          <MaximumCurrent>Infinity</MaximumCurrent>
        </PowerRequirements>
      </Power>
      -->";
    }
}
