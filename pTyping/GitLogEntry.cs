using System;

namespace pTyping;

// ReSharper disable once ClassNeverInstantiated.Global
public class GitLogEntry {
	public string   Commit  { get; set; }
	public string   Author  { get; set; }
	public DateTime Date    { get; set; }
	public string   Message { get; set; }
}
