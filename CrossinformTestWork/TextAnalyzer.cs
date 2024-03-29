﻿using System;
using System.Collections.Generic;
using System.Threading;
using System.Text.RegularExpressions;

namespace CrossinformTestWork
{
    // Класс предоставляет функции для анализа текста
    class TextAnalyzer
    {
        public static Dictionary<string, int> FrequencyAnalysis(string text)
        {
            // Переводим текст в нижний регистр
            text = text.ToLower();

            // Заменяем символы перевода строк на пробел
            text = Regex.Replace(text, @"[\n\r\-\–]", " ");
            // Удаляем все знаки препинания и другие лишние символы
            text = Regex.Replace(text, @"[\.\,\:\;\!\?«»*…\[\]()\d]", "");
            
            // Разбиваем текст на слова
            string[] words = text.Split(' ');

            // Создаём словарь, для хранения частоты встречаемсости триплетов
            Dictionary<string, int> triplets = new Dictionary<string, int>();

            // Устанавливаем количество потоков равное количеству ядер процессора
            int threadCount = Environment.ProcessorCount;
            // Количество слов на 1 поток
            int wordsPerStream = (int) words.Length / threadCount;

            // Список для хранения запущенных потоков
            List<Thread> threads = new List<Thread>();
            // Список для хранения счетчиков триплетов
            List<TripletsCounter> tripletsCounters = new List<TripletsCounter>();

            // Запускаем потоки в работу
            for (int i = 0; i < threadCount; i++)
            {
                // Количество слов для работы конкретному потоку
                // Последнему потоку отдаем все оставшиеся слова
                int wordsCount;
                if (i == threadCount - 1)
                    wordsCount = words.Length - wordsPerStream * (i);
                else
                    wordsCount = wordsPerStream;

                // Создаем объект для подсчёта триплетов
                TripletsCounter counter = new TripletsCounter(new ArraySegment<String>(words, wordsPerStream * i, wordsCount));
                // Добавляем счетчик в список
                tripletsCounters.Add(counter);

                // Создаем поток
                Thread myThread = new Thread(new ThreadStart(counter.Count));
                // Добавляем поток в список
                threads.Add(myThread);

                // Запускаем поток
                myThread.Start();
            }

            //Указываем основному потоку дождаться окончания работы всех запущенных ранее потоков
            foreach (var thread in threads)
            {
                thread.Join();
            }

            // Объединяем результаты работы каждого потока в один словарь
            foreach (var counter in tripletsCounters)
            {
                foreach (var item in counter.GetTriplets())
                {
                    if (triplets.ContainsKey(item.Key))
                        triplets[item.Key] += item.Value;
                    else
                        triplets[item.Key] = item.Value;
                }
            }

            return triplets;
        }
    }
}
