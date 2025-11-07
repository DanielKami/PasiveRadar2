using System.Threading.Tasks;

namespace PasiveRadar
{
    class RadarCumulate
    {
        uint cumulateIndex;
        uint Columns;
        uint Rows;
        uint MaxAverage;

        float[] CumulateBuffer;

        public void Init(Flags flags)
        {
            Columns = flags.Columns;
            Rows = flags.Rows;
            MaxAverage = flags.MaxAverage;
            uint Frame = Columns * Rows;
            CumulateBuffer = new float[(MaxAverage + 1) * Frame];

        }

        public void Run(float[] data, float[] PostProc, int average)
        {
            // 1. Zabezpieczenie i indeksacja
            cumulateIndex++;
            if (cumulateIndex >= average) cumulateIndex = 0;

            uint Frame = Columns * Rows;
            int FrameInt = (int)Frame; // Używamy int dla zarządzanego kodu

            // CCR (Current Cumulation Region) w elementach
            int CCR = (int)cumulateIndex * FrameInt;

            // Ochrona przed przekroczeniem zakresu
            if (CumulateBuffer.Length < CCR + FrameInt || data.Length < FrameInt) return;
            if (PostProc.Length < FrameInt) return;

            // 2. KUMULACJA (Zoptymalizowane Kopiowanie)
            // Zastąpienie wolnego Parallel.For na szybkie Array.Copy (lub BlockCopy)
            // To eliminuje narzut wątkowy i jest niemal natychmiastowe.
            System.Array.Copy(data, 0, CumulateBuffer, CCR, FrameInt);

            // 3. UŚREDNIANIE (Zoptymalizowana Równoległość)
            float scale = 1.0f / average;

            // Zmienna lokalna, która zawiera stały współczynnik
            // Zmieniamy mnożnik na 1.0f / average, skalowanie przez 255 (jeśli to finalna konwersja do bajtów)
            // powinno być na samym końcu lub poza tą funkcją.
            // Zachowujemy 1.0f / average, aby zachować poprawność uśredniania.
            float one_ave = scale * 255.0f;

            // ELIMINACJA Array.Clear I RYWIZACJI:

            // Krok 3a: Kopiujemy pierwszą (lub ostatnio zapisaną) ramkę do bufora wyjściowego. 
            // To eliminuje potrzebę Array.Clear(PostProc) oraz fałszywe współdzielenie.
            // Używamy zoptymalizowanego kopiowania dla pierwszej ramki (bez skalowania).
            System.Array.Copy(CumulateBuffer, CCR, PostProc, 0, FrameInt);

            // Uśrednianie jest teraz równoległe po indeksach pikseli, a sekwencyjne po ramkach.
            // To zapobiega rywalizacji o bufor PostProc.
            System.Threading.Tasks.Parallel.For(0, FrameInt, i =>
            {
                // Lokalna zmienna dla sumy, inicjalizowana pierwszą skopiowaną wartością
                float sum = PostProc[i];

                // Sumujemy resztę ramek
                for (int j = 0; j < average; j++)
                {
                    int jF = j * FrameInt;

                    // Omijamy ramkę, którą już skopiowaliśmy (CCR)
                    if (jF != CCR)
                    {
                        sum += CumulateBuffer[i + jF];
                    }
                }

                // Finalne uśrednianie i skalowanie
                PostProc[i] = sum * one_ave;
            });
        }
    }
}
