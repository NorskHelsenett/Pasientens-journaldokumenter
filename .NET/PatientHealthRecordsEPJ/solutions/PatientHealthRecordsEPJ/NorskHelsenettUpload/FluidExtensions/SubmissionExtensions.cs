using NorskHelsenettUpload.Ihe.Mhd.Models;
using NorskHelsenettUpload.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Intrinsics.Arm;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace NorskHelsenettUpload.FluidExtensions;

public static class SubmissionExtensions
{
	public static DocumentSubmissionMetadata WithUniqueId(this DocumentSubmissionMetadata submission, Guid uniqueId)
	{
        submission.UniqueId = uniqueId; 
		return submission;
	}

	public static DocumentSubmissionMetadata WithSubmissionTime(this DocumentSubmissionMetadata submission, DateTime submissionTime)
	{
		submission.SubmissionTime = submissionTime;
		return submission;
	}

	public static DocumentSubmissionMetadata WithAvailabilityStatus(this DocumentSubmissionMetadata submission, string availabilityStatus)
	{
		submission.AvailabilityStatus = availabilityStatus;
		return submission;
	}

	public static DocumentSubmissionMetadata WithSourceId(this DocumentSubmissionMetadata submission, string sourceId)
	{
		submission.SourceId = sourceId; 
		return submission;
	}

	public static DocumentSubmissionMetadata WithHomeCommunityId(this DocumentSubmissionMetadata submission, string homeCommunityId)
	{
		submission.HomeCommunityId = homeCommunityId;
		return submission;
	}

	public static DocumentSubmissionMetadata WithPatientId(this DocumentSubmissionMetadata submission, string patientId)
	{
		submission.PatientId = patientId;
		return submission;
	}

	public static DocumentSubmissionMetadata WithTitle(this DocumentSubmissionMetadata submission, string title)
	{
		submission.Title = title;
		return submission;
	}

	public static DocumentSubmissionMetadata WithComments(this DocumentSubmissionMetadata submission, string comments)
	{
		submission.Comments = comments;
		return submission;
	}

	public static DocumentSubmissionMetadata WithAuthor(this DocumentSubmissionMetadata submission, Author author)
	{
		submission.Author.Add(author); 
		return submission;
	}


}

