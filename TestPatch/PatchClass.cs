﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using NeosModLoader;
using FrooxEngine;
using FrooxEngine.LogiX;
using FrooxEngine.UIX;
using BaseX;
using FrooxEngine.LogiX.Color;
using FrooxEngine.LogiX.Input;
using FrooxEngine.LogiX.Operators;

namespace DarkLogiXBrowser
{
    public class DarkLogiXPatcher : NeosMod
    {
        public override string Name => "Fancy Metal LogixBrowser";
        public override string Author => "Cyro/EuphieEuphoria";
        public override string Version => "1.0.5";

        //Fancy metallic material idea by EuphieEuphoria, the actual materials were made by Gareth48, Gareth and Badhaloninja helped Euphie modify cyro's code

        //Define our constant Uris for our custom assets here
        static Uri fontUri = new Uri("neosdb:///0b82ab5fdba8e0147e38e89237ea4a430f0d7017c313d9b8e56a309acde756c0.ttf");
        static Uri spriteUri = new Uri("neosdb:///de19aa1ee297ae9ed9d3c7a05d20013b1a81c69e6560ac6be8caef0fd1186204.png");
        static Uri buttonPressClipUri = new Uri("neosdb:///30b37e0c34979b5244b686b464e2573bdc5e8291cf90bc0e5f5ca95fc485ce9f");
        static Uri buttonReleaseClipUri = new Uri("neosdb:///6910b354cc7aafd8f0e7a3817fde0460bb6290de4d39803a8dec5fb7d1a33ab6");
        static Uri buttonHoverEnterUri = new Uri("neosdb:///a8d42dd3b361127483dec673e934421ddfe3e29f9bad1e37f56e878a66fcf325");
        static Uri panelNormalMapUri = new Uri("neosdb:///95ef1fd8a153ad3d4c2588563274f961da94b812f90ffb4a235e624684c8e332");
        static Uri panelMSMapUri = new Uri("neosdb:///a38400e37e4e6b96d2e49557e0c7f614475edef637b2474f4c017f7e3f4971dc");
        static Uri backPanelAlbedoMapUri = new Uri("neosdb:///79e4969ec397d1d5de46cbc475b94725617351f0b1d559421b32670cc11a1d5a.png");

        public override void OnEngineInit()
        {
            Harmony harmony = new Harmony("net.cyro.TestPatch");
            harmony.PatchAll();

        }

        //This patch will patch the "GenerateMenuItems" function on the LogiX tip, allowing us to modify it.
        [HarmonyPatch(typeof(LogixTip), "GenerateMenuItems")]
        class LogiXTipPatcher
        {

            //This postfix handles changing the icon on the context menu from blue to grey, signifying that we've made the selector more awesome.
            static void Postfix(CommonTool tool, ContextMenu menu)
            {
                //Use harmony's access tools (e.g. it's shorthand reflection) to get the place where all of the arcs are stored.
                Slot ArcSlot = (AccessTools.Field(typeof(ContextMenu), "_itemsRoot").GetValue(menu) as SyncRef<Slot>).Target;

                //Here, we're using the LocalStringDrivers found on all of the arcs to get the arc we want.
                foreach (LocaleStringDriver t in ArcSlot.GetComponentsInChildren<LocaleStringDriver>())
                {
                    if (t.Key.Value == "Tooltip.Logix.NodeBrowser")
                    {
                        //If we find our target localestring, we can get the slot of that, and then get that slot's parent to get the arc slot.
                        ContextMenuItem ArcItem = t.Slot.Parent.GetComponent<ContextMenuItem>();
                        ArcItem.Color.Value = new color(0.2f, 0.2f, 0.2f, 0.75f);

                        //Here we're finding the image on the arc so that we can set it to our new awesome-indicating texture.
                        t.Slot.Parent.FindChild((Slot c) => c.Name == "Image").GetComponent<StaticTexture2D>().URL.Value = new Uri("neosdb:///c227e1ffc363a88c3521fd4a76004bb5d4ec1c5dd27b4729a68c73cb426bfc6e.webp");
                        break;
                    }

                }
            }
        }

        //This patch will patch the "BuildUI" function on the LogiX menu. This is where the buttons are generated when you click to a new directory
        [HarmonyPatch(typeof(LogixNodeSelector), "BuildUI")]
        class LogixNodeSelectorButtonPatcher
        {
            //This postfix will go back and awesome-ify all of the buttons
            [HarmonyPostfix]
            static void PrettifyButtons(ref LogixNodeSelector __instance, bool genericType)
            {
                //Check if the slot that the node menu component is on equals a specific value. This way you don't accidentally corrupt anybody else's node browsers if you attempt to use them
                if (__instance.Slot.Name != "Fancy Metal NodeMenu")
                    return;

                //Instantiate a bunch of variables that I will now painstakingly comment.

                //Shorthand for the slot that the node selector is on
                Slot BaseSlot = __instance.Slot;
                //Find the slot containing the button roots
                Slot ContainerSlot = __instance.Slot.FindChild((Slot c) => c.Name == "Content").FindChild((Slot c) => c.Name == "Container");
                //Find the slot containing all of the buttons
                Slot ContentSlot = ContainerSlot[ContainerSlot.ChildrenCount - 1].FindChild((Slot c) => c.Name == "Scroll Area")[0];
                //Find the slot that holds the ButtonAudioClipPlayer (for our awesome buttons to make sound when you touch them)
                Slot ButtonSoundsSlot = __instance.Slot.FindChild((Slot c) => c.Tag == "DarkUtil.Sounds");
                //Find the assets slot where we're caching all of our assets
                Slot AssetsSlot = __instance.Slot.FindChild((Slot c) => c.Tag == "DarkUtil.Assets");
                //Get our fancy sprite provider
                var FancySprite = AssetsSlot.GetComponent<SpriteProvider>();
                //Get our fancy font
                var FancyFont = AssetsSlot.GetComponent<StaticFont>();

                //Iterate over all of the buttons
                foreach (Slot c in ContentSlot.Children)
                {

                    if (!genericType)
                    {
                        //Find the image component on the currently iterated button
                        var Image = c.GetComponent<Image>();
                        //Get the tint and transform it in the HSV space so that it's grey, yet still has an indicator of it's luminosity so that there's contrast on the different button types
                        /*color val = Image.Tint.Value;
                        ColorHSV TransformColor = new ColorHSV(val);
                        ColorHSV NewTransformColor = new ColorHSV(0.7f, 0f, (TransformColor.h + 1.3f) / 4f, 1f).ToRGB();
                        Image.Tint.Value = NewTransformColor;*/

                        //Place our awesome sprite onto the button
                        Image.Sprite.Target = FancySprite;
                    }


                    //Make the buttons single-click instead of double-click
                    c.GetComponent<ButtonRelay<string>>().DoublePressDelay.Value = 0f;

                    //Find the text slot, get the text component, set it to our fancy font and change the text color to white
                    Slot TextSlot = c[0];
                    Text text = TextSlot.GetComponent<Text>();
                    text.Font.Target = FancyFont;
                    //text.Color.Value = new color(1f, 1f, 0.8f, 1f);

                    //Slightly adjust the transform of the text so that it's not touching the edges of the buttons any more.
                    RectTransform rect = TextSlot.GetComponent<RectTransform>();

                    rect.AnchorMin.Value = new float2(0.03f, 0f);
                    rect.AnchorMax.Value = new float2(0.97f, 1f);

                    //Create hover and press event relays that point back to the sound slot containing the ButtonAudioClipPlayer so that our buttons are clicky. Also saves putting a player on every button
                    var PressRelay = c.AttachComponent<ButtonPressEventRelay>();
                    var HoverRelay = c.AttachComponent<ButtonHoverEventRelay>();

                    //Temporarily commented out due to annoyance
                    PressRelay.Target.Target = ButtonSoundsSlot;
                    HoverRelay.Target.Target = ButtonSoundsSlot;

                }
            }

        }

        //This patch will patch the "OnAttach" function of the node browser, allowing us to do some setup whenever the node menu is spawned
        [HarmonyPatch(typeof(LogixNodeSelector), "OnAttach")]
        class LogixNodeSelectorPatcher
        {
            //This prefex will handle setting up the slot name and creating our asset/sound caches.
            static void Prefix(ref LogixNodeSelector __instance)
            {
                //Set the slot name to the one we want (we check this in another part of the code to be sure it's actually our node menu)
                __instance.Slot.Name = "Fancy Metal NodeMenu";

                //Create our assets cache and attach our awesome font to it. We'll also tag it so we can find it later
                Slot Assets = __instance.Slot.AddSlot("Assets");
                Assets.Tag = "DarkUtil.Assets";
                StaticFont staticFont = Assets.AttachFont(fontUri);
                staticFont.GlyphEmSize.Value = 32;

                //Create our awesome sprite and attach it to the asset cache
                SpriteProvider FancySprite = Assets.AttachSprite(spriteUri);
                FancySprite.Scale.Value = 0.04f;
                FancySprite.Borders.Value = new float4(0.25f, 0.25f, 0.25f, 0.25f);

                //Create our button clip player cache cache and tag it so that we can find it later
                Slot Sounds = __instance.Slot.AddSlot("Button Sounds");
                Sounds.Tag = "DarkUtil.Sounds";

                //Attach the ButtonAudioClipPlayer component so that we can have our buttons make fun noises. Also set the oneshot clips to parent under the node browser (littering root is bad!)
                var ButtonSounds = Sounds.AttachComponent<ButtonAudioClipPlayer>();
                ButtonSounds.ParentUnder.Target = __instance.Slot;

                //Add a new entry to the pressing sounds and set it up with the audio clip that will play when the button is pressed.
                var press = ButtonSounds.PressedClips.Add();
                press.Clip.Target = Assets.AttachAudioClip(buttonPressClipUri);
                press.MinVolume.Value = 0.15f;
                press.MaxVolume.Value = 0.15f;
                press.MinSpeed.Value = 0.75f;
                press.MaxSpeed.Value = 1.25f;

                //Add a new entry to the releasing sounds and set it up with the audio clip that will play when the button is released.
                var release = ButtonSounds.ReleasedClips.Add();
                release.Clip.Target = Assets.AttachAudioClip(buttonReleaseClipUri);
                release.MinVolume.Value = 0.15f;
                release.MaxVolume.Value = 0.15f;
                release.MinSpeed.Value = 0.75f;
                release.MaxSpeed.Value = 1.25f;

                //Add a new entry to the hover enter sounds and set it up with the audio clip that will play when the you start hovering over the button
                var hover = ButtonSounds.HoverEnterClips.Add();
                hover.Clip.Target = Assets.AttachAudioClip(buttonHoverEnterUri);
                hover.MinVolume.Value = 0.15f;
                hover.MaxVolume.Value = 0.15f;
                hover.MinSpeed.Value = 0.75f;
                hover.MaxSpeed.Value = 1.25f;
            }

            //This post fix will go back and turn the meshes on the node browser dark
            [HarmonyPostfix]
            static void PrettifyBrowser(ref LogixNodeSelector __instance)
            {
                //Check if the slot name is a specific value so that we don't run this if the slot isn't named what we named it
                if (__instance.Slot.Name != "Fancy Metal NodeMenu")
                    return;

                //Instantiate a bunch of constants that I will now painstakingly comment

                //Find the panel
                Slot slot = __instance.Panel.Slot.FindChild(ch => ch.Name.Equals("Panel"), 1);
                //Get the handle
                Slot HandleSlot = __instance.Panel.Slot.FindChild(ch => ch.Name.Equals("Handle"), 1);
                //Get the title bar
                Slot TitleSlot = __instance.Panel.Slot.FindChild(ch => ch.Name.Equals("Title Mesh"), 2);
                //Get the text on the title bar
                Slot TitleText = __instance.Panel.Slot.FindChild(ch => ch.Name.Equals("Title"), 2);
                //Find our assets cache
                Slot AssetsSlot = __instance.Slot.FindChild(ch => ch.Tag.Equals("DarkUtil.Assets"));
                //Find our ButtonAudioClipPlayer slot
                Slot Sounds = __instance.Slot.FindChild(c => c.Tag.Equals("DarkUtil.Sounds"));
                //Get the ButtonAudioClipPlayer
                var ButtonSounds = Sounds.GetComponent<ButtonAudioClipPlayer>();
                //Create a new material so that we can make our node menu dark with it
                PBS_TriplanarMetallic NewPanelMat = AssetsSlot.AttachComponent<PBS_TriplanarMetallic>(true, null);
                //Get our awesome font
                StaticFont staticFont = AssetsSlot.GetComponent<StaticFont>();
                //Making the static texture for the normal map
                StaticTexture2D panelNormalMap = AssetsSlot.AttachComponent<StaticTexture2D>(true, null);
                panelNormalMap.URL.Value = panelNormalMapUri;
                panelNormalMap.IsNormalMap.Value = true;
                panelNormalMap.CrunchCompressed.Value = false;
                panelNormalMap.PreferredFormat.Value = CodeX.TextureCompression.RawRGBA;
                panelNormalMap.FilterMode.Value = TextureFilterMode.Anisotropic;
                panelNormalMap.AnisotropicLevel.Value = 16;
                //Making the static texture for the MetallicSmoothness map
                StaticTexture2D panelMSMap = AssetsSlot.AttachComponent<StaticTexture2D>(true, null);
                panelMSMap.URL.Value = panelMSMapUri;
                panelMSMap.FilterMode.Value = TextureFilterMode.Anisotropic;
                panelMSMap.AnisotropicLevel.Value = 16;

                //Set up values on the material that are dark, destroy the blur renderers and set the materials on all of the meshes to our dark one. Also set the title font to our awesome font
                NewPanelMat.AlbedoColor.Value = new color(0.7686275f, 0.7803922f, 0.7803922f, 1f);
                NewPanelMat.ObjectSpace.Value = true;
                NewPanelMat.NormalMap.Target = panelNormalMap;
                NewPanelMat.MetallicMap.Target = panelMSMap;
                NewPanelMat.TextureScale.Value = new float2(4f, 4f);


                PBS_TriplanarMetallic gold = AssetsSlot.DuplicateComponent<PBS_TriplanarMetallic>(NewPanelMat, false);
                gold.AlbedoColor.Value = new color(1f, 0.89f, 0.61f, 1f);
                slot.GetComponents<MeshRenderer>(null, false)[0].Material.Target = NewPanelMat;
                slot.GetComponents<MeshRenderer>(null, false)[1].Destroy();
                HandleSlot.GetComponents<MeshRenderer>(null, false)[0].Material.Target = gold;
                HandleSlot.GetComponents<MeshRenderer>(null, false)[1].Destroy();
                TitleSlot.GetComponents<MeshRenderer>(null, false)[0].Material.Target = gold;
                TextRenderer textRenderer = TitleText.GetComponents<TextRenderer>(null, false)[0];
                textRenderer.Font.Target = staticFont;
                textRenderer.Color.Value = new color(1f, 1f, 1f, 1f);
                textRenderer.Size.Value = .9f;

                //Set up a little bit of LogiX that will ensure that the button sounds don't play until the sliding animation for switching directories is completed
                //This just checks if there are two "Content" slots under the container, indicating that the slide animation is playing. If there's only one, it enables the button sounds
                var SlotRef = Sounds.AttachComponent<ReferenceNode<Slot>>();
                var ChildCount = Sounds.AttachComponent<FrooxEngine.LogiX.WorldModel.ChildrenCount>();
                var GreaterThan = Sounds.AttachComponent<FrooxEngine.LogiX.Operators.LessThan_Int>();
                var DriveNode = Sounds.AttachComponent<DriverNode<bool>>();
                var ConstInt = Sounds.AttachComponent<FrooxEngine.LogiX.Data.ValueRegister<int>>();
                Slot ContainerSlot = __instance.Slot.FindChild((Slot c) => c.Name == "Container", 2);

                //You can probably figure out how these all connect on your own, after all I can't explain everything to you, where's the challenge in that? :)
                ConstInt.Value.Value = 2;
                SlotRef.RefTarget.Target = ContainerSlot;
                ChildCount.Instance.Target = SlotRef;
                GreaterThan.A.Target = ChildCount;
                GreaterThan.B.Target = ConstInt;

                DriveNode.Source.Target = GreaterThan;
                DriveNode.DriveTarget.Target = ButtonSounds.EnabledField;

                StaticTexture2D backSpriteTexture = AssetsSlot.AttachComponent<StaticTexture2D>(true, null);
                backSpriteTexture.URL.Value = backPanelAlbedoMapUri;
                backSpriteTexture.FilterMode.Value = TextureFilterMode.Anisotropic;
                backSpriteTexture.AnisotropicLevel.Value = 16;
                UnlitMaterial backSpriteUnlit = AssetsSlot.AttachComponent<UnlitMaterial>(true, null);
                backSpriteUnlit.Texture.Target = backSpriteTexture;
                backSpriteUnlit.TintColor.Value = new color(1.25f, 1.25f, 1.25f, 1f);
                backSpriteUnlit.BlendMode.Value = BlendMode.Alpha;


                Slot backSprite = slot.AddSlot("Back Panel Sprite");
                QuadMesh coolBackMesh = backSprite.AttachMesh<QuadMesh>(backSpriteUnlit, false, 0);
                coolBackMesh.Size.Value = new float2(.4f, .4f);
                backSprite.LocalPosition = new float3(0f, 0f, .0053f);
                backSprite.LocalRotation = floatQ.Euler(0f, 180f, 0f);

                Slot colorDriver = backSprite.AddSlot("Color Driver");

                List<IField<color>> colorTargets = new List<IField<color>>();
                colorTargets.Add(coolBackMesh.UpperLeftColor);
                colorTargets.Add(coolBackMesh.LowerLeftColor);
                colorTargets.Add(coolBackMesh.LowerRightColor);
                colorTargets.Add(coolBackMesh.UpperRightColor);

                var T = colorDriver.AttachComponent<TimeNode>();
                var TMulti = colorDriver.AttachComponent<Mul_Float>();
                var TMultiValue = colorDriver.AttachComponent<ValueNode<float>>();
                TMultiValue.Value.Value = .25f;

                var colorRot = .25f;
             
                var colorSaturation = colorDriver.AttachComponent<ValueNode<float>>();
                colorSaturation.Value.Value = .75f;

                var colorValue = colorDriver.AttachComponent<ValueNode<float>>();
                colorValue.Value.Value = 1f;


                TMulti.A.Target = T;
                TMulti.B.Target = TMultiValue;

                for (int i = 0; i < colorTargets.Count; i++)
                {
                    var colorRotHolder = colorDriver.AttachComponent<ValueNode<float>>();
                    colorRotHolder.Value.Value = colorRot * i;
                    var addition = colorDriver.AttachComponent<Add_Float>();
                    addition.A.Target = TMulti;
                    addition.B.Target = colorRotHolder;
                    var hsv = colorDriver.AttachComponent<HSV_ToColor>();
                    hsv.H.Target = addition;
                    hsv.S.Target = colorSaturation;
                    hsv.V.Target = colorValue;
                    var driver = colorDriver.AttachComponent<DriverNode<color>>();
                    driver.Source.Target = hsv;
                    driver.DriveTarget.Target = colorTargets[i];

                }

            }
        }


    }
}
