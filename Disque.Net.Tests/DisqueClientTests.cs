﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Common.Testing.NUnit;
using FluentAssertions;
using NUnit.Framework;

namespace Disque.Net.Tests
{
    //DOCKERFILE: https://registry.hub.docker.com/u/squiidz/disque/
    public class DisqueClientTests : TestBase
    {
        private ISyncDisqueClient q;

        protected override void FinalizeSetUp()
        {
            q = new DisqueClient(new Uri("disque://192.168.59.103:7711"));
        }

        protected override void FinalizeTearDown()
        {
            q.Close();
        }

        [Test]
        public void Ping()
        {
            string pong = q.Ping();
            pong.Should().NotBeNullOrEmpty();
        }

        [Test]
        public void AddJob()
        {
            string jobId = q.AddJob(GetQueueName(), "message", 10);
            jobId.Should().NotBeNullOrEmpty();
        }

        [Test]
        public void AddJobWithParams()
        {
            JobParams jobParams = new JobParams
            {
                Replicate = 1,
                Retry = 10,
                Ttl = 20,
                Maxlen = 10,
                Delay = 10,
                Async = true
            };

            string jobId = q.AddJob(GetQueueName(), "message", 10, jobParams);
            jobId.Should().NotBeNullOrEmpty();
        }

        [Test]
        public void GetJob()
        {
            string queue = GetQueueName();
            string jobId = q.AddJob(queue, "test", 60);

            Thread.Sleep(2000);

            List<Job> jobs = q.GetJob(new List<string> { queue });
            Job job = jobs.First();

            jobId.Should().Be(job.Id);
            "test".Should().Be(job.Body);
            queue.Should().Be(job.QueueName);
        }

        [Test]
        public void GetJobWithParams()
        {
            String queue = GetQueueName();
            q.AddJob(queue, "message", 10);
            q.AddJob(queue, "message", 10);
            List<Job> jobs = q.GetJob(100, 2, new List<string> { queue });
            jobs.Count.Should().Be(2);
        }

        [Test]
        public void AckJob()
        {
            string jobId = q.AddJob(GetQueueName(), "message", 10);
            long count = q.Ackjob(jobId);

            count.Should().Be(1);
        }

        [Test]
        public void FastAck()
        {
            string jobId = q.AddJob("fastack", "message", 10);
            long count = q.Fastack(jobId);

            count.Should().Be(1);
        }

        [Test]
        public void Info()
        {
            string info = q.Info();
            info.Should().NotBeNullOrEmpty();
        }

        [Test]
        public void Info_By_Section()
        {
            string info = q.Info("server");
            info.Should().NotBeNullOrEmpty();
        }


        [Test]
        public void Qlen()
        {
            string queue = GetQueueName();
            long qlen = q.Qlen(queue);
            qlen.Should().Be(0);
        }

        [Test]
        [Ignore("pending")]
        public void Hello()
        {

        }

        [Test]
        [Ignore("pending (not yet implemented)")]
        public void Qstat()
        {
        }

        [Test]
        public void Qpeek()
        {
            // We're testing also the response parsing here
            string queue = GetQueueName();

            q.AddJob(queue, "testJob", 10);
            q.AddJob(queue, "testJob2", 10);

            List<Job> jobs = q.Qpeek(queue, 2);

            Job job = jobs.First();
            Job job2 = jobs.Last();

            job.Body.Should().Be("testJob");
            job2.Body.Should().Be("testJob2");
        }

        [Test]
        public void QpeekEmpty()
        {
            List<Job> jobs = q.Qpeek(GetQueueName(), 2);
            jobs.Count.Should().Be(0);
        }

        [Test]
        public void QpeekInverse()
        {
            String queue = GetQueueName();
            List<Job> jobs = q.Qpeek(queue, -2);
            jobs.Count.Should().Be(0);
        }

        [Test]
        public void Enqueue()
        {
            string queue = GetQueueName();
            string jobId = q.AddJob(queue, "testJob", 10);
            long count = q.Enqueue(jobId);
            count.Should().Be(0);
        }

        [Test]
        public void Dequeue()
        {
            string queue = GetQueueName();
            string jobId = q.AddJob(queue, "testJob", 10);
            long count = q.Dequeue(jobId);
            count.Should().Be(1);
        }

        [Test]
        public void DelJob()
        {
            string queue = GetQueueName();
            string jobId = q.AddJob(queue, "testJob", 10);
            long count = q.DelJob(jobId);
            count.Should().Be(1);
        }

        [Test]
        public void Show()
        {
            string queue = GetQueueName();
            string jobId = q.AddJob(queue, "testJob", 10);
            JobInfo info = q.Show(jobId);
            info.Should().NotBeNull();
        }


        [Test]
        [Ignore("TODO: ")]
        public void Working()
        {
            String queue = GetQueueName();
            String jobId = q.AddJob(queue, "testJob", 10);
            long secs = q.Working(jobId);
            secs.Should().NotBe(null);
            secs.Should().NotBe(0L);
        }

        [Test]
        [Ignore("pending")]
        public void Scan()
        {
        }

        private static string GetQueueName()
        {
            return Guid.NewGuid().ToString("N");
        }
    }
}