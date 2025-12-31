#!/usr/bin/env ruby

require 'net/http'
require 'uri'
require 'json'
require 'cgi'
require 'base64'
require './utils'

$stdout.sync = true

################### Main #######################
load_env("../.env")

url = "https://register-dev.please-scan.com/napbiotec/customer-user-create/9b79ebc1-6976-4209-adf3-d18ebc963e1c?data=eyJFbWFpbCI6InBqYW1lLmZiMkBnbWFpbC5jb20iLCJOYW1lIjoiXHUwRTE3XHUwRTE0XHUwRTJBXHUwRTJEXHUwRTFBXHUwRTFFXHUwRTM1XHUwRTQ4XHUwRTQwXHUwRTA4XHUwRTIxXHUwRTJBXHUwRTRDIiwiQ29kZSI6Ilx1MEUxN1x1MEUxNFx1MEUyQVx1MEUyRFx1MEUxQVx1MEUxRVx1MEUzNVx1MEU0OFx1MEU0MFx1MEUwOFx1MEUyMVx1MEUyQVx1MEU0QyIsIklkIjoiNmFiZmRmYTYtMDk3Mi00NmFhLWE3OGQtYTZjNTAwNTFjNTAwIn0%3d"
uri = URI.parse(url)

# แปลง query string เป็น hash
params = CGI.parse(uri.query)
data = params['data'].first

path = uri.path
parts = path.split('/').reject(&:empty?)

decoded = Base64.decode64(data)
dataObj = JSON.parse(decoded)

orgId = parts[0]
regType = parts[1]
token = parts[2]

api = "ConfirmCreateCustomerUser"

#puts(dataObj)
custId = dataObj['Id']

#puts(dataObj)
userName = dataObj['UserName']

param =  {
  Email: dataObj['Email'],
  Password: "#{ENV['USER_PASSWORD']}",
}

ENV['API_KEY'] = nil # ถ้าไม่ใช้ API KEY ก็เซ็ตเป็น nil

apiUrl = "api/Registration/org/#{orgId}/action/#{api}/#{token}/#{custId}"
#puts(apiUrl)

result = make_request(:post, apiUrl, param)
puts(result)
