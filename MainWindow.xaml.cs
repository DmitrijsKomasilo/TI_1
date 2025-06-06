using Microsoft.Win32;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace InfoTheoryLab
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }


        private void ReadFileButton_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog()
            {
                Title = "Выберите текстовый файл",
                Filter = "Текстовые файлы (*.txt)|*.txt|Все файлы (*.*)|*.*"
            };

            if (dlg.ShowDialog() == true)
            {
                try
                {
                    string text = File.ReadAllText(dlg.FileName, Encoding.UTF8);
                    SourceTextBox.Text = text;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Не удалось прочитать файл:\n{ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void EncryptButton_Click(object sender, RoutedEventArgs e)
        {
            string rawKey = KeyTextBox.Text;
            string rawSource = SourceTextBox.Text;
            int algIdx = AlgorithmComboBox.SelectedIndex;

            if (algIdx == 0)
            {

                string filteredDigits = new string(rawKey.Where(char.IsDigit).ToArray());
                if (string.IsNullOrEmpty(filteredDigits))
                {
                    MessageBox.Show("Ключ для Rail Fence должен содержать хотя бы одну цифру (> 0).", "Ошибка ключа", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (!int.TryParse(filteredDigits, out int rails) || rails <= 0)
                {
                    MessageBox.Show("Ключ для Rail Fence должен быть числом больше 0.", "Ошибка ключа", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                string filteredPlain = FilterEnglish(rawSource);

                string cipher = RailFenceEncrypt(filteredPlain, rails);
                ResultTextBox.Text = cipher;
            }
            else
            {
                string filteredKeyRus = new string(rawKey
                    .Where(c => IsRussianLetter(c))
                    .Select(c => char.ToUpper(c))
                    .ToArray());

                if (string.IsNullOrEmpty(filteredKeyRus))
                {
                    MessageBox.Show("Ключ для Виженера должен содержать хотя бы одну букву русского алфавита.", "Ошибка ключа", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                string filteredPlainRus = new string(rawSource
                    .Where(c => IsRussianLetter(c))
                    .Select(c => char.ToUpper(c))
                    .ToArray());

                string cipher = VigenereEncrypt(filteredPlainRus, filteredKeyRus);
                ResultTextBox.Text = cipher;
            }
        }

        private void DecryptButton_Click(object sender, RoutedEventArgs e)
        {
            string rawKey = KeyTextBox.Text;
            string rawSource = SourceTextBox.Text;
            int algIdx = AlgorithmComboBox.SelectedIndex;

            if (algIdx == 0)
            {
                string filteredDigits = new string(rawKey.Where(char.IsDigit).ToArray());
                if (string.IsNullOrEmpty(filteredDigits))
                {
                    MessageBox.Show("Ключ для Rail Fence должен содержать хотя бы одну цифру (> 0).", "Ошибка ключа", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (!int.TryParse(filteredDigits, out int rails) || rails <= 0)
                {
                    MessageBox.Show("Ключ для Rail Fence должен быть числом больше 0.", "Ошибка ключа", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                string filteredCipher = FilterEnglish(rawSource);

                string plain = RailFenceDecrypt(filteredCipher, rails);
                ResultTextBox.Text = plain;
            }
            else
            {
                string filteredKeyRus = new string(rawKey
                    .Where(c => IsRussianLetter(c))
                    .Select(c => char.ToUpper(c))
                    .ToArray());

                if (string.IsNullOrEmpty(filteredKeyRus))
                {
                    MessageBox.Show("Ключ для Виженера должен содержать хотя бы одну букву русского алфавита.", "Ошибка ключа", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                string filteredCipherRus = new string(rawSource
                    .Where(c => IsRussianLetter(c))
                    .Select(c => char.ToUpper(c))
                    .ToArray());

                string plain = VigenereDecrypt(filteredCipherRus, filteredKeyRus);
                ResultTextBox.Text = plain;
            }
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            KeyTextBox.Clear();
            SourceTextBox.Clear();
            ResultTextBox.Clear();
        }


        private static string FilterEnglish(string input)
        {
            var sb = new StringBuilder();
            foreach (char c in input)
            {
                if (c >= 'A' && c <= 'Z')
                {
                    sb.Append(c);
                }
                else if (c >= 'a' && c <= 'z')
                {
                    sb.Append(char.ToUpperInvariant(c));
                }
            }
            return sb.ToString();
        }


        private static bool IsRussianLetter(char c)
        {
            return (c >= 'А' && c <= 'Я') ||
                   (c >= 'а' && c <= 'я') ||
                   c == 'Ё' ||
                   c == 'ё';
        }



        private static string RailFenceEncrypt(string plain, int rails)
        {
            if (rails == 1 || plain.Length <= rails)
            {
                return plain;
            }

            StringBuilder[] fence = new StringBuilder[rails];
            for (int i = 0; i < rails; i++)
                fence[i] = new StringBuilder();

            int currentRail = 0;
            bool goingDown = true;

            foreach (char c in plain)
            {
                fence[currentRail].Append(c);

                if (currentRail == 0)
                    goingDown = true;
                else if (currentRail == rails - 1)
                    goingDown = false;

                currentRail += goingDown ? 1 : -1;
            }

            var cipherSb = new StringBuilder();
            foreach (var railSb in fence)
                cipherSb.Append(railSb.ToString());

            return cipherSb.ToString();
        }

        private static string RailFenceDecrypt(string cipher, int rails)
        {
            if (rails == 1 || cipher.Length <= rails)
            {
                return cipher;
            }

            int length = cipher.Length;
            int[] railLengths = new int[rails];
            int currentRail = 0;
            bool goingDown = true;
            for (int i = 0; i < length; i++)
            {
                railLengths[currentRail]++;
                if (currentRail == 0)
                    goingDown = true;
                else if (currentRail == rails - 1)
                    goingDown = false;
                currentRail += goingDown ? 1 : -1;
            }

            string[] railChunks = new string[rails];
            int pos = 0;
            for (int r = 0; r < rails; r++)
            {
                railChunks[r] = cipher.Substring(pos, railLengths[r]);
                pos += railLengths[r];
            }

            var resultSb = new StringBuilder();
            int[] railCounters = new int[rails];
            currentRail = 0;
            goingDown = true;
            for (int i = 0; i < length; i++)
            {
                resultSb.Append(railChunks[currentRail][railCounters[currentRail]]);
                railCounters[currentRail]++;

                if (currentRail == 0)
                    goingDown = true;
                else if (currentRail == rails - 1)
                    goingDown = false;

                currentRail += goingDown ? 1 : -1;
            }

            return resultSb.ToString();
        }



        private static readonly char[] RussianAlphabet =
        {
            'А','Б','В','Г','Д','Е','Ё','Ж','З','И','Й','К','Л','М','Н','О','П',
            'Р','С','Т','У','Ф','Х','Ц','Ч','Ш','Щ','Ъ','Ы','Ь','Э','Ю','Я'
        };

        private static string VigenereEncrypt(string plain, string key)
        {
            var result = new StringBuilder();
            int m = RussianAlphabet.Length;
            int keyLen = key.Length;

            for (int i = 0; i < plain.Length; i++)
            {
                char p = plain[i];
                char k = key[i % keyLen];
                int pi = Array.IndexOf(RussianAlphabet, p);
                int ki = Array.IndexOf(RussianAlphabet, k);
                int ci = (pi + ki) % m;
                result.Append(RussianAlphabet[ci]);
            }

            return result.ToString();
        }

        private static string VigenereDecrypt(string cipher, string key)
        {
            var result = new StringBuilder();
            int m = RussianAlphabet.Length;
            int keyLen = key.Length;

            for (int i = 0; i < cipher.Length; i++)
            {
                char c = cipher[i];
                char k = key[i % keyLen];
                int ci = Array.IndexOf(RussianAlphabet, c);
                int ki = Array.IndexOf(RussianAlphabet, k);
                int pi = (ci - ki + m) % m;
                result.Append(RussianAlphabet[pi]);
            }

            return result.ToString();
        }
    }
}
