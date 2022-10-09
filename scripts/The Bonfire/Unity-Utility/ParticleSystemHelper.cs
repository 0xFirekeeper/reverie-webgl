// Filename: ParticleSystemHelper.cs
// Author: 0xFirekeeper
// Description: Can Set the Tint of the Start Color and Color Over Lifetime modules of a particle system

using UnityEngine;

public static class ParticleSystemHelper
{
    public static void SetModuleTint(ParticleSystem ps, Color wantedTint, bool tintWhiteAndBlack = true, bool oldHue = false, bool oldSaturation = true, bool oldValue = true, Color wantedSecondTint = new Color())
    {
        // H S V Extracting

        float oldH, oldS, oldV, newH, newS, newV, finalH, finalS, finalV;

        Color.RGBToHSV(wantedTint, out newH, out newS, out newV);

        finalH = newH;
        finalS = newS;
        finalV = newV;

        oldH = oldS = oldV = 1.0f;

        #region Start Color Handling

        var mainModule = ps.main;

        Color firstColor = Color.white, secondColor = wantedSecondTint;
        Gradient firstGradient = new Gradient(), secondGradient = new Gradient();

        switch (mainModule.startColor.mode)
        {
            case (ParticleSystemGradientMode.Color):
                Debug.Log("ParticleSystemGradientMode for Start Color: Color");

                if (!tintWhiteAndBlack && (ColorUtility.ToHtmlStringRGB(mainModule.startColor.color) == "FFFFFF" || ColorUtility.ToHtmlStringRGB(mainModule.startColor.color) == "000000"))
                    break;

                Color.RGBToHSV(mainModule.startColor.color, out oldH, out oldS, out oldV);

                if (oldHue)
                    finalH = oldH;
                if (oldSaturation)
                    finalS = oldS;
                if (oldValue)
                    finalV = oldV;

                firstColor = Color.HSVToRGB(finalH, finalS, finalV);

                mainModule.startColor = new ParticleSystem.MinMaxGradient(firstColor);
                break;
            case (ParticleSystemGradientMode.Gradient):
                Debug.Log("ParticleSystemGradientMode for Start Color: Gradient");

                Gradient currentGrad = mainModule.startColor.gradient;

                GradientColorKey[] tempColors = new GradientColorKey[currentGrad.colorKeys.Length];
                GradientAlphaKey[] tempAlphas = currentGrad.alphaKeys;

                for (int i = 0; i < tempColors.Length; i++)
                {
                    Color.RGBToHSV(currentGrad.colorKeys[i].color, out oldH, out oldS, out oldV);

                    if (oldHue)
                        finalH = oldH;
                    if (oldSaturation)
                        finalS = oldS;
                    if (oldValue)
                        finalV = oldV;

                    Color finalColor = Color.HSVToRGB(finalH, finalS, finalV);

                    if (!tintWhiteAndBlack && (ColorUtility.ToHtmlStringRGB(currentGrad.colorKeys[i].color) == "FFFFFF" || ColorUtility.ToHtmlStringRGB(currentGrad.colorKeys[i].color) == "000000"))
                        finalColor = currentGrad.colorKeys[i].color;

                    tempColors[i] = new GradientColorKey(finalColor, currentGrad.colorKeys[i].time);
                }

                firstGradient.SetKeys(
                    tempColors,
                    tempAlphas
                );

                mainModule.startColor = new ParticleSystem.MinMaxGradient(firstGradient);
                break;
            case (ParticleSystemGradientMode.RandomColor):
                Debug.Log("ParticleSystemGradientMode for Start Color: RandomColor");

                Gradient randomGrad = mainModule.startColor.gradient;

                GradientColorKey[] randomColorKeys = new GradientColorKey[randomGrad.colorKeys.Length];
                GradientAlphaKey[] randomAlphaKeys = randomGrad.alphaKeys;

                for (int i = 0; i < randomColorKeys.Length; i++)
                {
                    Color.RGBToHSV(randomGrad.colorKeys[i].color, out oldH, out oldS, out oldV);

                    if (oldHue)
                        finalH = oldH;
                    if (oldSaturation)
                        finalS = oldS;
                    if (oldValue)
                        finalV = oldV;

                    Color finalColor = Color.HSVToRGB(finalH, finalS, finalV);

                    if (!tintWhiteAndBlack && (ColorUtility.ToHtmlStringRGB(randomGrad.colorKeys[i].color) == "FFFFFF" || ColorUtility.ToHtmlStringRGB(randomGrad.colorKeys[i].color) == "000000"))
                        finalColor = randomGrad.colorKeys[i].color;

                    randomColorKeys[i] = new GradientColorKey(finalColor, randomGrad.colorKeys[i].time);
                }

                firstGradient.SetKeys(
                    randomColorKeys,
                    randomAlphaKeys
                );

                mainModule.startColor = new ParticleSystem.MinMaxGradient(firstGradient);
                break;
            case (ParticleSystemGradientMode.TwoColors):
                Debug.Log("ParticleSystemGradientMode for Start Color: TwoColors");

                if (!tintWhiteAndBlack && (ColorUtility.ToHtmlStringRGB(mainModule.startColor.colorMin) == "FFFFFF" || ColorUtility.ToHtmlStringRGB(mainModule.startColor.colorMin) == "000000"))
                    goto skipColorMin;

                Color.RGBToHSV(mainModule.startColor.colorMin, out oldH, out oldS, out oldV);

                if (oldHue)
                    finalH = oldH;
                if (oldSaturation)
                    finalS = oldS;
                if (oldValue)
                    finalV = oldV;

                firstColor = Color.HSVToRGB(finalH, finalS, finalV);
                Debug.Log("Set First Color");


            skipColorMin:

                if (!tintWhiteAndBlack && (ColorUtility.ToHtmlStringRGB(mainModule.startColor.colorMax) == "FFFFFF" || ColorUtility.ToHtmlStringRGB(mainModule.startColor.colorMax) == "000000"))
                    goto skipColorMax;

                Color.RGBToHSV(mainModule.startColor.colorMax, out oldH, out oldS, out oldV);

                if (oldHue)
                    finalH = oldH;
                if (oldSaturation)
                    finalS = oldS;
                if (oldValue)
                    finalV = oldV;

                secondColor = Color.HSVToRGB(finalH, finalS, finalV);

                Debug.Log("Set Second Color");

            skipColorMax:

                mainModule.startColor = new ParticleSystem.MinMaxGradient(firstColor, secondColor);

                break;
            case (ParticleSystemGradientMode.TwoGradients):
                Debug.Log("ParticleSystemGradientMode for Start Color: TwoGradients");

                // First Grad
                Gradient currentFirstGrad = mainModule.startColor.gradientMin;

                GradientColorKey[] firstGradColors = new GradientColorKey[currentFirstGrad.colorKeys.Length];
                GradientAlphaKey[] firstGradAlphas = currentFirstGrad.alphaKeys;

                for (int i = 0; i < firstGradColors.Length; i++)
                {
                    Color.RGBToHSV(currentFirstGrad.colorKeys[i].color, out oldH, out oldS, out oldV);

                    if (oldHue)
                        finalH = oldH;
                    if (oldSaturation)
                        finalS = oldS;
                    if (oldValue)
                        finalV = oldV;

                    Color finalColor = Color.HSVToRGB(finalH, finalS, finalV);

                    if (!tintWhiteAndBlack && (ColorUtility.ToHtmlStringRGB(currentFirstGrad.colorKeys[i].color) == "FFFFFF" || ColorUtility.ToHtmlStringRGB(currentFirstGrad.colorKeys[i].color) == "000000"))
                        finalColor = currentFirstGrad.colorKeys[i].color;

                    firstGradColors[i] = new GradientColorKey(finalColor, currentFirstGrad.colorKeys[i].time);
                }

                firstGradient.SetKeys(
                    firstGradColors,
                    firstGradAlphas
                );

                // Second Grad
                Gradient currentSecondGrad = mainModule.startColor.gradientMax;

                GradientColorKey[] secondGradColors = new GradientColorKey[currentSecondGrad.colorKeys.Length];
                GradientAlphaKey[] secondGradAlphas = currentSecondGrad.alphaKeys;

                for (int i = 0; i < secondGradColors.Length; i++)
                {
                    Color.RGBToHSV(currentSecondGrad.colorKeys[i].color, out oldH, out oldS, out oldV);

                    if (oldHue)
                        finalH = oldH;
                    if (oldSaturation)
                        finalS = oldS;
                    if (oldValue)
                        finalV = oldV;

                    Color finalColor = Color.HSVToRGB(finalH, finalS, finalV);

                    if (!tintWhiteAndBlack && (ColorUtility.ToHtmlStringRGB(currentSecondGrad.colorKeys[i].color) == "FFFFFF" || ColorUtility.ToHtmlStringRGB(currentSecondGrad.colorKeys[i].color) == "000000"))
                        finalColor = currentSecondGrad.colorKeys[i].color;

                    secondGradColors[i] = new GradientColorKey(finalColor, currentSecondGrad.colorKeys[i].time);
                }

                secondGradient.SetKeys(
                    secondGradColors,
                    secondGradAlphas
                );

                mainModule.startColor = new ParticleSystem.MinMaxGradient(firstGradient, secondGradient);
                break;
        }

        #endregion

        #region Color Over Lifetime Handling

        var colOverLifetimeModule = ps.colorOverLifetime;

        if (colOverLifetimeModule.enabled != true)
            return;

        Gradient oldGrad = ps.colorOverLifetime.color.gradient;

        GradientColorKey[] newColors = new GradientColorKey[oldGrad.colorKeys.Length];
        GradientAlphaKey[] newAlphas = oldGrad.alphaKeys;

        for (int i = 0; i < newColors.Length; i++)
        {

            Color.RGBToHSV(oldGrad.colorKeys[i].color, out oldH, out oldS, out oldV);

            if (oldHue)
                finalH = oldH;
            if (oldSaturation)
                finalS = oldS;
            if (oldValue)
                finalV = oldV;

            Color finalColor = Color.HSVToRGB(finalH, finalS, finalV);

            if (!tintWhiteAndBlack && (ColorUtility.ToHtmlStringRGB(oldGrad.colorKeys[i].color) == "FFFFFF" || ColorUtility.ToHtmlStringRGB(oldGrad.colorKeys[i].color) == "000000"))
                finalColor = oldGrad.colorKeys[i].color;

            newColors[i] = new GradientColorKey(finalColor, oldGrad.colorKeys[i].time);
        }

        Gradient newGrad = new Gradient();

        newGrad.SetKeys(
            newColors,
            newAlphas
        );

        colOverLifetimeModule.color = newGrad;

        #endregion

    }
}