#!/usr/bin/env ruby

require 'net/http'
require 'uri'
require 'json'
require './utils'

$stdout.sync = true

################### Main #######################
load_env("../.env")

orgId = ENV['API_ORG']
id = 'c0edbdf8-be23-452d-a5f4-27e484e0152e' #'67b1ad7c-8f62-42d4-80e8-2e3de9012785' # 81d68ff0-0d66-440e-91f6-5cf9dd219aa9
param = nil

### Inviteuser
apiUrl = "api/OrganizationUser/org/#{orgId}/action/GetForgotPasswordLink/#{id}"
result = make_request(:get, apiUrl, param)
puts(result)

