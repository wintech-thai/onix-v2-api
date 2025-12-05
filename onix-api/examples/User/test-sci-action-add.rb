#!/usr/bin/env ruby

require 'net/http'
require 'uri'
require 'json'
require './utils'

$stdout.sync = true

################### Main #######################
load_env("../.env")

orgId = ENV['API_ORG']

apiUrl = "api/ScanItemAction/org/#{orgId}/action/AddScanItemAction"
param =  {
  ActionName: "test1-action",
  Description: "description action",
  EncryptionKey: "sdsdsd1234567890",
  EncryptionIV: "acdses2345678901",
  ThemeVerify: "xxxxxxx",
  RegisteredAwareFlag: "YES",
  Tags: "defaultxx",
}

result = make_request(:post, apiUrl, param)
puts(result)
