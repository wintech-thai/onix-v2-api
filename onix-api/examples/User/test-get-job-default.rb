#!/usr/bin/env ruby

require 'net/http'
require 'uri'
require 'json'
require './utils'

$stdout.sync = true

################### Main #######################
load_env("../.env")

orgId = "napbiotec"
jobType = "ScanItemGenerator"

### GetJobDefault
apiUrl = "api/Job/org/#{orgId}/action/GetJobDefault/#{jobType}"

result = make_request(:get, apiUrl, nil)
puts(result)
