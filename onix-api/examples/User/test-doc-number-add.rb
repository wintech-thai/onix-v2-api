#!/usr/bin/env ruby

require 'net/http'
require 'uri'
require 'json'
require './utils'

$stdout.sync = true

################### Main #######################
load_env("../.env")

orgId = ENV['API_ORG']

apiUrl = "api/DocumentNumber/org/#{orgId}/action/AddDocumentNumberConfig"
param = {
  DocumentType: "TempDocumentNo",
  DocumentFormat: "TMP-${yyyy}-${mm}-${seq}",
  SeqDigit: 4,
  ResetType: "Monthly",
  YearOffset: 0,
  Tags: "testing,temp",
}

result = make_request(:post, apiUrl, param)

json = result.to_json
puts(json)
