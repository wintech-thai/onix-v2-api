#!/usr/bin/env ruby

require 'net/http'
require 'uri'
require 'json'
require './utils'

$stdout.sync = true

################### Main #######################
load_env(".env")

orgId = ENV['API_ORG']
id = '04ae7602-15b6-4e8e-9744-18efd0aca730'

param =  {
  Roles: [ "VIEWER" ],
}

### UpdateUserById
apiUrl = "api/OrganizationUser/org/#{orgId}/action/UpdateUserById/#{id}"
result = make_request(:post, apiUrl, param)

puts(result)
